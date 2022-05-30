// for details on configuring this project to bundle and minify static web assets.
// Write your JavaScript code.
// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification


    function inform(msg) {
        document.getElementById("info").innerHTML = msg;
    }

    function showBoard(bool) {
        document.getElementById("mainArea").classList.toggle("hidden", !bool);
        document.getElementById("showButton").classList.toggle("hidden", bool);
    }

    function getShipOrWater(val) {
        return val ? "🚢" : "🌊";
    }

    function getHitOrMiss(isShip) {
        return isShip ? "🎯" : "🌫️";
    }

    function displayVictory(name) {
        inform(name + " has won!");
        document.getElementById("buttons").classList.toggle("hidden");
        setInterval(slowlyRotate, 40);

        function RandColor() {
            var r = Math.floor(Math.random() * 255);
            var g = Math.floor(Math.random() * 255);
            var b = Math.floor(Math.random() * 255);
            return `rgb(${r},${g},${b})`;
        }

        for (var y = 0; y < 10; y++) {
            for (var x = 0; x < 10; x++) {
                var btnA = document.getElementById("btA" + x + ";" + y);
                var btnB = document.getElementById("btB" + x + ";" + y);
                btnA.style.borderColor = RandColor();
                btnB.style.borderColor = RandColor();
            }
        }
    }

    rotation = 0;

    function slowlyRotate() {
        var table = document.getElementById("PlayerField");
        table.setAttribute("style", "transform: scale(" + (1 - rotation/100) + ")" + " rotate(" + rotation + "deg);");
        table = document.getElementById("EnemyField");
        table.setAttribute("style", "transform: scale(" + (1 + rotation/100)+ ");");
        rotation++;
        if (rotation >= 3600) rotation = -3600;
    }

    function sendAjax(data, handler, onreturn, onfail) {
        $.ajax({
            type: "GET",
            url: '/?handler=' + handler,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: data,
            success: onreturn,
            error: onfail
        });
    }

    function getCoords(id) {
        var coords = id.substring(3).split(";");
        return { x: parseInt(coords[0]), y: parseInt(coords[1]) };
    }

    function sendShip() {
        var btn = this;
        sendAjax(
            getCoords(this.id),
            "AddShip",
            function(response) {
                var rsp = JSON.parse(response);
                if (rsp.Invalid) {
                    if (rsp.MaxShipsAchieved) inform("Max ships already placed.");
                    else inform("Space already occupied.");
                }
                else {
                    btn.innerHTML = "🚢";
                    inform("Ships placed: " + rsp.Ships + "/" + rsp.MaxShips);
                }
            }, 
            function(response) {inform("Connection failed.");}
        );
    }

    function sendHit() {
        var btn = this;
        sendAjax(
            getCoords(this.id),
            "Shoot",
            function(response) {
                var rsp = JSON.parse(response);
                if (rsp.Invalid) {
                    inform("Can't shoot now.");
                }
                else {
                    btn.innerHTML = rsp.Occupied ? "🎯" : "🌫️";
                    if (rsp.Won) displayVictory(rsp.Player);
                }
            }, 
            function(response) {inform("Connection failed.");}
        );
    }

    function setState(ships, tries, enemyShips, enemyTries) {
        showBoard(false);
        for (var y = 0; y < 10; y++) {
            for (var x = 0; x < 10; x++) {
                var btnA = document.getElementById("btA" + x + ";" + y);
                var btnB = document.getElementById("btB" + x + ";" + y);
                btnA.innerHTML = getShipOrWater(ships[y][x]);
                if (enemyTries[y][x])
                    btnA.innerHTML = getHitOrMiss(ships[y][x]);
                if (tries[y][x])
                    btnB.innerHTML = getHitOrMiss(enemyShips[y][x]);
                else
                    btnB.innerHTML = "🌊";
            }
        }
    }

    function sendSwap() {
        sendAjax(
            {}, "Swap",
            function(response) {
                var rsp = JSON.parse(response);
                if (rsp.Invalid) {
                    inform("Can't swap yet.");
                }
                else {
                    setState(rsp.Ships, rsp.Tries, rsp.EnemyShips, rsp.EnemyTries);
                    inform(rsp.Player + "'s turn.");
                }
            },
            function() {inform("Connection failed");}
        );
    }

    function sendRandom() {
        sendAjax({}, "Random",
        function(response) {
            var rsp = JSON.parse(response);
            if (!rsp.Invalid) {
                document.getElementById("btA" + rsp.x + ";" + rsp.y).innerHTML = "🚢";
                inform("Ships placed: " + rsp.Ships + "/" + rsp.MaxShips);
            }
            else {
                inform("Can't place any more.");
            }
        },
        function() {console.log("Connection failed");});
    }

    function createTable(table, diff, func) {
        for (var y = 0; y < 10; y++) {
            var row = document.createElement("tr");
            for (var x = 0; x < 10; x++) {
                var td = document.createElement("td");
                var btn = document.createElement("button");
                btn.id = "bt" + diff +  x + ";" + y;
                btn.innerHTML = "🌊";
                btn.onclick = func;
                td.append(btn);
                row.append(td);
            }
            table.append(row);
        }
    }

    document.addEventListener("DOMContentLoaded", function() {
            defined = true;
            var table = document.getElementById("PlayerField");
            createTable(table, "A", sendShip);
            var table = document.getElementById("EnemyField");
            createTable(table, "B", sendHit);
        
            sendAjax(
                {NameA : "Player1", NameB : "Player2"},
                "Reload",
                function() {}, function() {}
            );
    });
