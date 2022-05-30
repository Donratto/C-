using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace BS01.Pages;

[IgnoreAntiforgeryToken(Order = 2000)]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    static IndexModel() {
        CurrentPlayer = new Player("A");
        OtherPlayer = new Player("B");
    }

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    static Player CurrentPlayer, OtherPlayer;

    static JsonResult GetAsJson(object o) {
        var str = JsonSerializer.Serialize(o);
        return new JsonResult(str);
    }

    public void OnGetReload(PlayerMsg msg) {
        CurrentPlayer = new Player(msg.NameA);
        OtherPlayer = new Player(msg.NameB);
        Ready = false;
        HasShot = false;
    }

    public JsonResult OnGetAddShip(ButtonPos msg) {
        if (Ready) return GetAsJson(new ShipResponse() { Invalid = true, MaxShipsAchieved = true });

        var occupied = !CurrentPlayer.AddShip(msg.x, msg.y);

        var occ = new ShipResponse()
        {
            Occupied = occupied,
            Ships = CurrentPlayer.ShipsLeft,
            Invalid = occupied == true,
            MaxShipsAchieved = CurrentPlayer.ShipsLeft == Player.MaxShips,
            Player = CurrentPlayer.Name,
            MaxShips = Player.MaxShips
        };
        return GetAsJson(occ);
    }

    static bool HasShot = false;
    static bool Ready = false;

    private static bool[][] ToMultiArray(bool[,] ar) {
        bool[][] ret = new bool[10][];
        for (int y = 0; y < 10; y++) {
            ret[y] = new bool[10];
            for (int x = 0; x < 10; x++) {
                ret[y][x] = ar[x,y];
            }
        }
        return ret;
    }

    public JsonResult OnGetSwap() {
        if (!CurrentPlayer.Ready || (Ready && !HasShot))
            return GetAsJson(new ShipResponse() {Invalid = true});

        HasShot = false;
        var temp = CurrentPlayer;
        CurrentPlayer = OtherPlayer;
        OtherPlayer = temp;

        if (CurrentPlayer.Ready) Ready = true;

        return GetAsJson(new TableResponse() {
            Ships = ToMultiArray(CurrentPlayer.Occupied),
            Tries = ToMultiArray(CurrentPlayer.Tried),
            EnemyShips = ToMultiArray(OtherPlayer.Occupied),
            EnemyTries = ToMultiArray(OtherPlayer.Tried),
            Player = CurrentPlayer.Name
        });
    }

    public JsonResult OnGetShoot(ButtonPos msg) {
        if (HasShot || !Ready) return GetAsJson(new ShipResponse() {Invalid = true});

        var valid = CurrentPlayer.TryShoot(msg.x, msg.y);
        if (!valid) return GetAsJson(new ShipResponse() {Invalid = true});

        var occ = new ShipResponse()
        {
            Occupied = OtherPlayer.GetShot(msg.x, msg.y),
            Invalid = !valid,
            Won = OtherPlayer.ShipsLeft == 0,
            Player = CurrentPlayer.Name
        };
        HasShot = true;
        return GetAsJson(occ);
    }

    public JsonResult OnGetRandom() {
        if (Ready) return GetAsJson(new ButtonResponse() {Invalid = true});

        var suc = CurrentPlayer.SetRandom(out var x, out var y);
        return GetAsJson(new ButtonResponse() {
            x = x, y = y, Invalid = !suc,
            Ships = CurrentPlayer.ShipsLeft,
            MaxShips = Player.MaxShips
        });
    }
}

public class Player {
    public bool[,] Occupied = new bool[10,10];
    public bool[,] Tried = new bool[10,10];
    public string Name;
    public int ShipsLeft = 0;
    public static int MaxShips = 2;

    public bool Ready = false;

    public Player(string name) {
        Name = name;
    }

    public bool TryShoot(int x, int y) {
        if (Tried[x,y]) return false;
        else return Tried[x,y] = true;
    }

    public bool GetShot(int x, int y)
    {
        if (Occupied[x,y]) {
            ShipsLeft--;
            return true;
        }
        else return false;
    }

    public bool AddShip(int x, int y)
    {
        if (ShipsLeft == MaxShips) return false;

        if (Occupied[x,y]) return false;
        else
        {
            Occupied[x,y] = true;
            ShipsLeft++;
            if (ShipsLeft == MaxShips) Ready = true;
            return true;
        }
    }

    public bool SetRandom(out int x, out int y) {
        x = y = 0;
        if (ShipsLeft == MaxShips) return false;
        Random rnd = new();
        bool set = false;
        while (!set) {
            x = rnd.Next(10);
            y = rnd.Next(10);
            if (!Occupied[x,y]) {
                set = true;
                ShipsLeft++;
                Occupied[x,y] = true;
            }
        }
        if (ShipsLeft == MaxShips) Ready = true;
        return true;
    }
}

    public class PlayerMsg {
        public string NameA {get;set;} = "A";
        public string NameB {get;set;} = "B";
    }

public class ButtonPos {
        public int x {get; set;} = 0;
        public int y {get; set;} = 0;
    }

    public class ButtonResponse {
        public int x {get; set;} = 0;
        public int y {get; set;} = 0;
        public bool Invalid {get;set;} = false;
        public int Ships { get; set; } = 0;
        public int MaxShips { get; set; } = 20;
    }

    public class ShipResponse {
        public bool Occupied { get; set; } = false;
        public int Ships { get; set; } = 0;
        public int MaxShips { get; set; } = 20;
        public bool Won {get;set;} = false;
        public bool Invalid {get;set;} = false;
        public bool MaxShipsAchieved {get;set;} = false;
        public string Player {get; set;} = "A";
    }

    public class TableResponse {
        public bool[][]? Ships {get; set;}
        public bool[][]? EnemyShips {get; set;}
        public bool[][]? Tries {get; set;}
        public bool[][]? EnemyTries {get;set;}
        public string Player {get; set;} = "A";
    }