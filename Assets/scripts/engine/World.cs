using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class TileDictEntry {
    public string id;
    public Transform tile;
}

[System.Serializable]
public class SymbolMapping {
    public string id;
    public string symbol;
    public bool collide;
}

[System.Serializable]
public class DoorMapping {
    public string id;
    public string symbol;
    public int nextRoom;
    public int spawnId;
}

[System.Serializable]
public class SpawnMapping {
    public string symbol;
    public int id;
}

[System.Serializable]
public class Room {
    public List<DoorMapping> doors;
    public List<SpawnMapping> spawns;
    public List<string> background;
    public List<string> level;
    //public List<string> foreground;
    //public string bgPath;
}

[System.Serializable]
public class Map {
    public string airSymbol;
    public int spawnId;
    public List<SymbolMapping> mappings;
    public List<Room> rooms;
}

public class World : MonoBehaviour {

    public static World instance;

    public List<TileDictEntry> tileDictEntires;
    Dictionary<string, Transform> tileDictionary = new Dictionary<string, Transform>();

    public string mapName;
    public Map defaultMap;
    public Transform character;

    Transform background;
    Transform level;

    Transform player;

    Dictionary<int, Vector2> spawns = new Dictionary<int, Vector2>();

    Map currentMap;
    int currentRoom = 0;

	public void Awake() {

        if(instance == null)
            instance = this;

        background = this.transform.Find("Background");
        level = this.transform.Find("Level");

        foreach(TileDictEntry e in tileDictEntires)
            tileDictionary.Add(e.id, e.tile);

        File.WriteAllText(Application.dataPath + Path.DirectorySeparatorChar + "levels" + Path.DirectorySeparatorChar + "default.json", JsonUtility.ToJson(defaultMap));

        currentMap = JsonUtility.FromJson<Map>(File.ReadAllText(Application.dataPath + Path.DirectorySeparatorChar + "levels" + Path.DirectorySeparatorChar + this.mapName + ".json"));
        
        if(currentMap == null)
            currentMap = defaultMap;

        loadRoom();
        spawn(currentMap.spawnId);
    }

    public void loadRoom() {
        Room room = currentMap.rooms[currentRoom];
        loadRoomSection(background, room.background.ToArray());
        loadRoomSection(level, room.level.ToArray(), true);
    }

    public void destroyRoom() {

        spawns.Clear();

        var toDelete = new List<GameObject>();

        toDelete.Add(player.gameObject);
        foreach(Transform child in this.background) toDelete.Add(child.gameObject);
        foreach(Transform child in this.level) toDelete.Add(child.gameObject);

        toDelete.ForEach(child => Destroy(child));
    }

    public void loadRoomSection(Transform parent, string[] map, bool collide = false) {

        Room room = currentMap.rooms[currentRoom];

        // first, make sure their all the same length
        int length = map[0].Length;
        foreach(string s in map)
            if(s.Length != length) return;

        Vector2 offset = new Vector2(-map[0].Length / 2, map.Length / 2);

        // then, orient everything.
        for(int y = 0; y < map.Length; y++) {
            for(int x = 0; x < map[y].Length; x++) {

                char symbol = map[y][x];

                if(symbol == currentMap.airSymbol[0])
                    continue;

                SpawnMapping smap = room.spawns.Find(a => a.symbol[0] == symbol);
                if(smap != null) {
                    spawns.Add(smap.id, new Vector2(offset.x + x, offset.y - y));
                }

                string id;

                int nextRoom = -2;
                int spawnId = 0;

                bool tile_collide = false;

                DoorMapping emap = room.doors.Find(a => a.symbol[0] == symbol);
                if(emap != null) {

                    id = emap.id;
                    nextRoom = emap.nextRoom;
                    spawnId = emap.spawnId;

                } else {
                    SymbolMapping mapping = currentMap.mappings.Find(a => a.symbol[0] == symbol);
                    if(mapping == null) {
                        print("can't find symbol: " + symbol);
                        continue;
                    }

                    id = mapping.id;
                    tile_collide = mapping.collide;
                }

                Transform prefab;
                if(tileDictionary.TryGetValue(id, out prefab)) {
                    Transform tile = Instantiate(prefab);
                    tile.SetParent(parent, false);
                    tile.localPosition = new Vector3(offset.x + x, offset.y - y, 0);
                    if(collide) {
                        if(tile_collide)
                            tile.gameObject.AddComponent<BoxCollider2D>();
                        tile.gameObject.layer = LayerMask.NameToLayer("Level");
                    }

                    if(nextRoom > -2) {
                        ExitHandler eh = tile.GetComponent<ExitHandler>();
                        eh.room = nextRoom;
                        eh.spawnId = spawnId;
                    }

                } else print("can't find prefab for: " + id);
            }
        }
    }

    public void spawn(int id) {

        Vector2 spawnLoc;
        if(spawns.TryGetValue(id, out spawnLoc)) {
            if(player != null)
                Destroy(player);

            player = Instantiate(character);
            player.position = spawnLoc;
        }
    }

    bool debounceSwitchRoom = false;
	public void switchRoom(int room, int spawnId) {
        if(debounceSwitchRoom)
            return;
        debounceSwitchRoom = true;
        if(room > -1 && room < currentMap.rooms.Count) {
            // fade
            currentRoom = room;
            destroyRoom();
            loadRoom();
            spawn(spawnId);
        } else if(room == -1) {
            // celebrate :P
            print("FINISHED");
            Application.Quit();
        } else {
            print("invalid room!");
        }
        debounceSwitchRoom = false;
    }
}
