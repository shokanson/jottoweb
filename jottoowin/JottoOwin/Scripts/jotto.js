/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/signalr/signalr.d.ts" />
var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var viewModel;
var gameHub;
var ajaxUtils;
var apiUtils;
var AjaxUtils = (function () {
    function AjaxUtils() {
    }
    AjaxUtils.prototype.getJson = function (url, success, error) {
        $.ajax({
            type: "GET",
            url: url,
            success: success,
            error: error
        });
    };
    AjaxUtils.prototype.postJson = function (url, data, success, error) {
        return $.ajax({
            type: "POST",
            url: url,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(data),
            success: success,
            error: error
        });
    };
    AjaxUtils.prototype.patchJson = function (url, data) {
        return $.ajax({
            type: "PATCH",
            url: url,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(data)
        });
    };
    return AjaxUtils;
}());
var ApiUtils = (function () {
    function ApiUtils() {
    }
    ApiUtils.prototype.startGame = function (player1Id, player2Id, word1) {
        ajaxUtils.postJson("api/games", { player1Id: player1Id, player2Id: player2Id, word1: word1 });
    };
    ApiUtils.prototype.registerPlayer = function (name, success) {
        ajaxUtils.getJson("api/players/" + name, success, function () {
            ajaxUtils.postJson("api/players", name, success);
        });
    };
    ApiUtils.prototype.getPlayer = function (playerId, success) {
        ajaxUtils.getJson("api/players/" + playerId, success);
    };
    ApiUtils.prototype.getPlayers = function (success) {
        ajaxUtils.getJson("api/players", success);
    };
    ApiUtils.prototype.makeGuess = function (gameId, playerId, forPlayerId, word, success, error) {
        ajaxUtils.postJson("api/games/" + gameId + "/guesses", { playerId: playerId, forPlayerId: forPlayerId, guess: word }, success, error);
    };
    ApiUtils.prototype.updateGameWord2 = function (gameId, word2) {
        ajaxUtils.patchJson("api/games/" + gameId, word2);
    };
    ApiUtils.prototype.getGameHelps = function (gameId, success) {
        ajaxUtils.getJson("api/games/" + gameId + "/helps", success);
    };
    ApiUtils.prototype.supplyHelperUnknownHint = function (gameId, playerId, letter, command, success, error) {
        var hint = { playerId: playerId, letter: letter, command: command };
        ajaxUtils.postJson("api/games/" + gameId + "/helps/unknown", hint, success, error);
    };
    ApiUtils.prototype.supplyHelperKnownInHint = function (gameId, playerId, letter, success, error) {
        var hint = { playerId: playerId, letter: letter, command: "" };
        ajaxUtils.postJson("api/games/" + gameId + "/helps/knownin", hint, success, error);
    };
    ApiUtils.prototype.supplyHelperKnownOutHint = function (gameId, playerId, letter, success, error) {
        var hint = { playerId: playerId, letter: letter, command: "" };
        ajaxUtils.postJson("api/games/" + gameId + "/helps/knownout", hint, success, error);
    };
    ApiUtils.prototype.resetHelps = function (gameId, playerId, success) {
        var hint = { playerId: playerId, letter: "", command: "reset" };
        ajaxUtils.postJson("api/games/" + gameId + "/helps", hint, success);
    };
    return ApiUtils;
}());
var ViewModel = (function () {
    function ViewModel(currentGameState, gameState, myId, myName, myWord, errorMessage, myGuesses, myJotto, opponents, gameId, inviterPlayer, invitedToGame, opponentId, opponentIsComputer, opponentGuesses, opponentJotto, guessToAdd, playerToRegister, helps, isConnected) {
        if (currentGameState === void 0) { currentGameState = ko.observable(); }
        if (gameState === void 0) { gameState = ko.observable(); }
        if (myId === void 0) { myId = ko.observable(""); }
        if (myName === void 0) { myName = ko.observable(""); }
        if (myWord === void 0) { myWord = ko.observable(""); }
        if (errorMessage === void 0) { errorMessage = ko.observable(""); }
        if (myGuesses === void 0) { myGuesses = ko.observableArray([]); }
        if (myJotto === void 0) { myJotto = ko.observable(false); }
        if (opponents === void 0) { opponents = ko.observableArray([]); }
        if (gameId === void 0) { gameId = ko.observable(""); }
        if (inviterPlayer === void 0) { inviterPlayer = ko.observable({ id: "", name: "", isComputer: false }); }
        if (invitedToGame === void 0) { invitedToGame = ko.observable({ id: "", player1Id: "", player2Id: "" }); }
        if (opponentId === void 0) { opponentId = ko.observable(""); }
        if (opponentIsComputer === void 0) { opponentIsComputer = ko.observable(false); }
        if (opponentGuesses === void 0) { opponentGuesses = ko.observableArray([]); }
        if (opponentJotto === void 0) { opponentJotto = ko.observable(false); }
        if (guessToAdd === void 0) { guessToAdd = ko.observable(""); }
        if (playerToRegister === void 0) { playerToRegister = ko.observable(""); }
        if (helps === void 0) { helps = ko.observable({ knownIn: "", knownOut: "", unknown: "", clues: [] }); }
        if (isConnected === void 0) { isConnected = ko.observable(false); }
        this.currentGameState = currentGameState;
        this.gameState = gameState;
        this.myId = myId;
        this.myName = myName;
        this.myWord = myWord;
        this.errorMessage = errorMessage;
        this.myGuesses = myGuesses;
        this.myJotto = myJotto;
        this.opponents = opponents;
        this.gameId = gameId;
        this.inviterPlayer = inviterPlayer;
        this.invitedToGame = invitedToGame;
        this.opponentId = opponentId;
        this.opponentIsComputer = opponentIsComputer;
        this.opponentGuesses = opponentGuesses;
        this.opponentJotto = opponentJotto;
        this.guessToAdd = guessToAdd;
        this.playerToRegister = playerToRegister;
        this.helps = helps;
        this.isConnected = isConnected;
        this.setGameState(new BeginState());
    }
    ViewModel.prototype.setGameState = function (state) {
        this.currentGameState(state);
        this.gameState(state.name);
    };
    ViewModel.prototype.playerAdded = function (player) {
        this.opponents.push(player);
    };
    ViewModel.prototype.gameUpdatedWord2 = function (game) {
        if (this.gameId() === game.id) {
            gameHub.server.joinGame(game.id); // subscribe for notifications about my game
            this.currentGameState().gameUpdatedWord2();
        }
    };
    ViewModel.prototype.gameStarted = function (game) {
        if (this.myId() === "") {
            this.invitedToGame(game); // maybe it's for me after I register
            return;
        }
        if (game.player1Id !== viewModel.myId() && game.player2Id !== viewModel.myId()) {
            return; // don't care about other folks' games
        }
        if (this.gameId() !== "") {
            return; // I'm already in a game, so don't join a new one
        }
        gameHub.server.joinGame(game.id); // subscribe for notifications about my game
        this.gameId(game.id);
        this.currentGameState().gameStarted(game);
        if (this.gameState() === "myTurn") {
            $("#guessTxt").focus();
        }
    };
    ViewModel.prototype.turnTaken = function (guess) {
        if (guess.playerId === this.myId()) {
            this.myGuesses.push(guess);
            if (guess.score === 6) {
                this.myJotto(true);
            }
        }
        else {
            this.opponentGuesses.push(guess);
            if (guess.score === 6) {
                this.opponentJotto(true);
            }
        }
        this.currentGameState().turnTaken(guess);
        if (this.gameState() === "myTurn") {
            $("#guessTxt").focus();
        }
    };
    return ViewModel;
}());
var StateBase = (function () {
    function StateBase() {
        //
    }
    Object.defineProperty(StateBase.prototype, "name", {
        get: function () { throw "not implemented"; },
        enumerable: true,
        configurable: true
    });
    StateBase.prototype.registerPlayer = function () { throw "registerPlayer not implemented"; };
    StateBase.prototype.chooseOpponent = function (player) { throw "chooseOpponent not implemented"; };
    StateBase.prototype.startGameAndWaitForOpponentWord = function () { throw "startGameAndWaitForOpponentWord not implemented"; };
    StateBase.prototype.waitForOpponentToStart = function () { throw "waitForOpponentToStart not implemented"; };
    StateBase.prototype.gameStarted = function (game) { throw "gameStarted not implemented"; };
    StateBase.prototype.joinGame = function () { throw "joinGame not implemented"; };
    StateBase.prototype.gameUpdatedWord2 = function () { throw "gameUpdatedWord2 not implemented"; };
    StateBase.prototype.takeTurn = function () { throw "takeTurn not implemented"; };
    StateBase.prototype.turnTaken = function (guess) { throw "turnTaken not implemented"; };
    StateBase.prototype.supplyUnknownHint = function (hint) { throw "supplyUnknownHint not implemented"; };
    StateBase.prototype.supplyKnownInHint = function (letter) { throw "supplyKnownInHint not implemented"; };
    StateBase.prototype.supplyKnownOutHint = function (letter) { throw "supplyKnownOutHint not implemented"; };
    StateBase.prototype.resetHelps = function () { throw "resetHelps not implemented"; };
    StateBase.prototype.resetGame = function () { throw "resetGame not implemented"; };
    return StateBase;
}());
var BeginState = (function (_super) {
    __extends(BeginState, _super);
    function BeginState() {
        var _this = _super.call(this) || this;
        console.log("BeginState");
        return _this;
    }
    Object.defineProperty(BeginState.prototype, "name", {
        get: function () { return "begin"; },
        enumerable: true,
        configurable: true
    });
    BeginState.prototype.registerPlayer = function () {
        console.log("BeginState.registerPlayer");
        if (!viewModel.isConnected()) {
            $.connection.hub.start();
        }
        if (viewModel.playerToRegister() !== "") {
            viewModel.errorMessage("");
            var playerName = viewModel.playerToRegister();
            apiUtils.registerPlayer(playerName, function (player) {
                viewModel.myId(player.id);
                viewModel.myName(player.name);
                if (viewModel.invitedToGame().player2Id === viewModel.myId()) {
                    viewModel.gameId(viewModel.invitedToGame().id);
                    apiUtils.getPlayer(viewModel.invitedToGame().player1Id, function (playerById) {
                        viewModel.inviterPlayer(playerById);
                    });
                    viewModel.invitedToGame({ id: "", player1Id: "", player2Id: "" });
                }
                apiUtils.getPlayers(function (players) {
                    viewModel.opponents(players.filter(function (playerToFilter) {
                        return playerToFilter.id !== viewModel.myId();
                    }));
                    viewModel.setGameState(new ChooseOpponentState());
                });
            });
            viewModel.playerToRegister("");
        }
    };
    return BeginState;
}(StateBase));
var ChooseOpponentState = (function (_super) {
    __extends(ChooseOpponentState, _super);
    function ChooseOpponentState() {
        var _this = _super.call(this) || this;
        console.log("ChooseOpponentState");
        return _this;
    }
    Object.defineProperty(ChooseOpponentState.prototype, "name", {
        get: function () { return "chooseOpponent"; },
        enumerable: true,
        configurable: true
    });
    ChooseOpponentState.prototype.chooseOpponent = function (player) {
        console.log("ChooseOpponentState.chooseOpponent");
        viewModel.opponentId(player.id);
        if (player.isComputer) {
            viewModel.opponentIsComputer(true);
            apiUtils.startGame(viewModel.myId(), viewModel.opponentId(), viewModel.myWord());
        }
        else {
            viewModel.setGameState(new ChooseWordState());
            $("#wordTxt").focus();
        }
    };
    ChooseOpponentState.prototype.gameStarted = function (game) {
        console.log("ChooseOpponentState.gameStarted");
        if (viewModel.opponentIsComputer()) {
            viewModel.setGameState(new MyTurnState());
        }
        else {
            apiUtils.getPlayer(game.player1Id, function (player) {
                viewModel.inviterPlayer(player);
                viewModel.setGameState(new InvitationPendingState());
            });
        }
    };
    return ChooseOpponentState;
}(StateBase));
var InvitationPendingState = (function (_super) {
    __extends(InvitationPendingState, _super);
    function InvitationPendingState() {
        var _this = _super.call(this) || this;
        console.log("InvitationPendingState");
        return _this;
    }
    Object.defineProperty(InvitationPendingState.prototype, "name", {
        get: function () { return "invitationPending"; },
        enumerable: true,
        configurable: true
    });
    InvitationPendingState.prototype.chooseOpponent = function (player) {
        console.log("InvitationPendingState.chooseOpponent");
        viewModel.opponentId(player.id);
        viewModel.setGameState(new ChooseWordState());
        $("#wordTxt").focus();
    };
    return InvitationPendingState;
}(StateBase));
var ChooseWordState = (function (_super) {
    __extends(ChooseWordState, _super);
    function ChooseWordState() {
        var _this = _super.call(this) || this;
        console.log("ChooseWordState");
        return _this;
    }
    Object.defineProperty(ChooseWordState.prototype, "name", {
        get: function () { return "chooseWord"; },
        enumerable: true,
        configurable: true
    });
    ChooseWordState.prototype.startGameAndWaitForOpponentWord = function () {
        console.log("ChooseWordState.startGameAndWaitForOpponentWord");
        if (viewModel.myId() === "") {
            throw "encountered invalid condition in ChooseWordState.startGameAndWaitForOpponentWord: myId is empty";
        }
        if (viewModel.opponentId() === "") {
            throw "encountered invalid condition in ChooseWordState.startGameAndWaitForOpponentWord: opponentId is empty";
        }
        if (viewModel.myWord() === "") {
            throw "encountered invalid condition in ChooseWordState.startGameAndWaitForOpponentWord: myWord is empty";
        }
        viewModel.setGameState(new WaitingForOpponentWordState());
        apiUtils.startGame(viewModel.myId(), viewModel.opponentId(), viewModel.myWord());
    };
    ChooseWordState.prototype.waitForOpponentToStart = function () {
        console.log("ChooseWordState.waitForOpponentToStart");
        if (viewModel.myWord() === "") {
            throw "encountered invalid condition in ChooseWordState.waitForOpponentToStart: myWord is empty";
        }
        if (viewModel.gameId() === "") {
            viewModel.setGameState(new WaitingForOpponentToStartState());
        }
        else {
            apiUtils.updateGameWord2(viewModel.gameId(), viewModel.myWord());
        }
    };
    ChooseWordState.prototype.gameStarted = function (game) {
        console.log("ChooseWordState.gameStarted");
        viewModel.setGameState(new WaitingForMyWordState());
    };
    ChooseWordState.prototype.joinGame = function () {
        console.log("ChooseWordState.joinGame");
        if (viewModel.gameId() === "") {
            throw "encountered invalid condition in WaitingForMyWordState.joinGame: gameId is empty";
        }
        if (viewModel.myWord() === "") {
            throw "encountered invalid condition in WaitingForMyWordState.joinGame: myWord is empty";
        }
        gameHub.server.joinGame(viewModel.gameId()); // subscribe for notifications about my game
        apiUtils.updateGameWord2(viewModel.gameId(), viewModel.myWord());
    };
    ChooseWordState.prototype.gameUpdatedWord2 = function () {
        console.log("ChooseWordState.gameUpdatedWord2");
        viewModel.setGameState(new OpponentTurnState());
    };
    return ChooseWordState;
}(StateBase));
var WaitingForOpponentToStartState = (function (_super) {
    __extends(WaitingForOpponentToStartState, _super);
    function WaitingForOpponentToStartState() {
        var _this = _super.call(this) || this;
        console.log("WaitingForOpponentToStartState");
        return _this;
    }
    Object.defineProperty(WaitingForOpponentToStartState.prototype, "name", {
        get: function () { return "waitingForOpponentToStart"; },
        enumerable: true,
        configurable: true
    });
    WaitingForOpponentToStartState.prototype.gameStarted = function (game) {
        console.log("WaitingForOpponentToStartState.gameStarted");
        if (game.id === "") {
            throw "encountered invalid condition in ChooseWordState.waitForOpponentToStart: game id is empty";
        }
        if (viewModel.myWord() === "") {
            throw "encountered invalid condition in ChooseWordState.waitForOpponentToStart: myWord is empty";
        }
        apiUtils.updateGameWord2(viewModel.gameId(), viewModel.myWord());
    };
    WaitingForOpponentToStartState.prototype.gameUpdatedWord2 = function () {
        console.log("WaitingForOpponentToStartState.gameUpdatedWord2");
        // let's try staying in this state and only switching to a new state once they've taken their turn
        //viewModel.setGameState(new OpponentTurnState());
    };
    WaitingForOpponentToStartState.prototype.turnTaken = function () {
        console.log("WaitingForOpponentToStartState.turnTaken");
        viewModel.setGameState(new MyTurnState());
    };
    return WaitingForOpponentToStartState;
}(StateBase));
var WaitingForOpponentWordState = (function (_super) {
    __extends(WaitingForOpponentWordState, _super);
    function WaitingForOpponentWordState() {
        var _this = _super.call(this) || this;
        console.log("WaitingForOpponentWordState");
        return _this;
    }
    Object.defineProperty(WaitingForOpponentWordState.prototype, "name", {
        get: function () { return "waitingForOpponentWord"; },
        enumerable: true,
        configurable: true
    });
    WaitingForOpponentWordState.prototype.gameUpdatedWord2 = function () {
        console.log("WaitingForOpponentWordState.gameUpdatedWord2");
        viewModel.setGameState(new MyTurnState());
    };
    WaitingForOpponentWordState.prototype.gameStarted = function (game) {
        console.log("WaitingForOpponentWordState.gameStarted");
        /*
         * Player 2 chooses opponent (Player 1), enters word, and chooses "wait for opponent to start"
         * Player 1 chooses opponent (Player 2), enters word, and chooses "Start game and wait for opponent word"
         *
         * In this scenario, Player 2's game will update the game with his word and go right to OpponentTurnState,
         * so we should go right to MyTurnState.
         *
         * Are there other scenarios where we should go to a different state, or stay in this state?
         */
        //viewModel.setGameState(new MyTurnState());
        /*
         * Player 1 chooses opponent (Player 2), enters word, and chooses "Start game and wait for opponent word"
         * Player 2 registers
         */
    };
    return WaitingForOpponentWordState;
}(StateBase));
var MyTurnState = (function (_super) {
    __extends(MyTurnState, _super);
    function MyTurnState() {
        var _this = _super.call(this) || this;
        console.log("MyTurnState");
        return _this;
    }
    Object.defineProperty(MyTurnState.prototype, "name", {
        get: function () { return "myTurn"; },
        enumerable: true,
        configurable: true
    });
    MyTurnState.prototype.takeTurn = function () {
        console.log("MyTurnState.takeTurn");
        if (viewModel.gameId() === "") {
            throw "encountered invalid condition in MyTurnState.takeTurn: game id is empty";
        }
        if (viewModel.myId() === "") {
            throw "encountered invalid condition in MyTurnState.takeTurn: my id is empty";
        }
        if (viewModel.opponentId() === "") {
            throw "encountered invalid condition in MyTurnState.takeTurn: opponent id is empty";
        }
        if (viewModel.guessToAdd() !== "") {
            apiUtils.makeGuess(viewModel.gameId(), viewModel.myId(), viewModel.opponentId(), viewModel.guessToAdd(), function () {
                viewModel.errorMessage("");
                apiUtils.getGameHelps(viewModel.gameId(), function (helps) {
                    if (helps.length > 0) {
                        viewModel.helps(helps[0]);
                    }
                });
            }, function (data) {
                var responseText = JSON.parse(JSON.stringify(data)).responseText;
                viewModel.errorMessage(JSON.parse(responseText).message);
            });
            viewModel.guessToAdd("");
        }
    };
    MyTurnState.prototype.turnTaken = function (guess) {
        console.log("MyTurnState.turnTaken");
        if (viewModel.opponentIsComputer()) {
            if (viewModel.myJotto()) {
                viewModel.setGameState(new DoneState());
            }
            // else just stay in MyTurnState
        }
        else {
            // since we're in MyTurnState, normally it would be their turn
            if (viewModel.opponentJotto()) {
                if (viewModel.myJotto()) {
                    // we've both got jotto, so we're done
                    viewModel.setGameState(new DoneState());
                }
                // else it would normally be their turn, but they've got jotto, so stay in MyTurnState
            }
            else {
                // normal state of affairs
                viewModel.setGameState(new OpponentTurnState());
            }
        }
    };
    MyTurnState.prototype.gameUpdatedWord2 = function () {
        console.log("MyTurnState.gameUpdatedWord2");
        // no-op: timing may cause this state to be entered prior to opponent providing their word
        // not sure if this is a problem yet.
    };
    MyTurnState.prototype.gameStarted = function (game) {
        console.log("MyTurnState.gameStarted");
        // no-op: ignore, 'cause I'm already in the middle of a game
    };
    MyTurnState.prototype.supplyUnknownHint = function (hint) {
        // hint comes in as letter followed by +/-, e.g. "e+", "q-", etc.
        console.log(hint);
        apiUtils.supplyHelperUnknownHint(viewModel.gameId(), viewModel.myId(), hint.substr(0, 1), hint.substr(1, 1), function () {
            apiUtils.getGameHelps(viewModel.gameId(), function (helps) {
                viewModel.errorMessage("");
                viewModel.helps(helps[0]);
                $("#guessTxt").focus();
            });
        }, function (data) {
            var responseText = JSON.parse(JSON.stringify(data)).responseText;
            viewModel.errorMessage(JSON.parse(responseText).message);
            $("#guessTxt").focus();
        });
    };
    MyTurnState.prototype.supplyKnownInHint = function (letter) {
        console.log(letter);
        apiUtils.supplyHelperKnownInHint(viewModel.gameId(), viewModel.myId(), letter, function () {
            apiUtils.getGameHelps(viewModel.gameId(), function (helps) {
                viewModel.errorMessage("");
                viewModel.helps(helps[0]);
                $("#guessTxt").focus();
            });
        }, function (data) {
            var responseText = JSON.parse(JSON.stringify(data)).responseText;
            viewModel.errorMessage(JSON.parse(responseText).message);
            $("#guessTxt").focus();
        });
    };
    MyTurnState.prototype.supplyKnownOutHint = function (letter) {
        console.log(letter);
        apiUtils.supplyHelperKnownOutHint(viewModel.gameId(), viewModel.myId(), letter, function () {
            apiUtils.getGameHelps(viewModel.gameId(), function (helps) {
                viewModel.errorMessage("");
                viewModel.helps(helps[0]);
                $("#guessTxt").focus();
            });
        }, function (data) {
            var responseText = JSON.parse(JSON.stringify(data)).responseText;
            viewModel.errorMessage(JSON.parse(responseText).message);
            $("#guessTxt").focus();
        });
    };
    MyTurnState.prototype.resetHelps = function () {
        apiUtils.resetHelps(viewModel.gameId(), viewModel.myId(), function () {
            apiUtils.getGameHelps(viewModel.gameId(), function (helps) {
                viewModel.errorMessage("");
                viewModel.helps(helps[0]);
                $("#guessTxt").focus();
            });
        });
    };
    return MyTurnState;
}(StateBase));
var OpponentTurnState = (function (_super) {
    __extends(OpponentTurnState, _super);
    function OpponentTurnState() {
        var _this = _super.call(this) || this;
        console.log("OpponentTurnState");
        return _this;
    }
    Object.defineProperty(OpponentTurnState.prototype, "name", {
        get: function () { return "opponentTurn"; },
        enumerable: true,
        configurable: true
    });
    OpponentTurnState.prototype.turnTaken = function (guess) {
        console.log("OpponentTurnState.turnTaken");
        if (viewModel.opponentIsComputer()) {
            throw "encountered invalid condition in OpponentTurnState.turnTaken: opponent can't be a computer";
        }
        else {
            // since we're in OpponentTurnState, normally it would be my turn
            if (viewModel.myJotto()) {
                if (viewModel.opponentJotto()) {
                    // we've both got jotto, so we're done
                    viewModel.setGameState(new DoneState());
                }
                // else it would normally be my turn, but I've got jotto, so stay in OpponentTurnState
            }
            else {
                // normal state of affairs
                viewModel.setGameState(new MyTurnState());
            }
        }
    };
    OpponentTurnState.prototype.gameStarted = function (game) {
        console.log("OpponentTurnState.gameStarted");
        // no-op: ignore, 'cause I'm already in the middle of a game
    };
    return OpponentTurnState;
}(StateBase));
var WaitingForMyWordState = (function (_super) {
    __extends(WaitingForMyWordState, _super);
    function WaitingForMyWordState() {
        var _this = _super.call(this) || this;
        console.log("WaitingForMyWordState");
        return _this;
    }
    Object.defineProperty(WaitingForMyWordState.prototype, "name", {
        get: function () { return "waitingForMyWord"; },
        enumerable: true,
        configurable: true
    });
    WaitingForMyWordState.prototype.joinGame = function () {
        console.log("WaitingForMyWordState.joinGame");
        if (viewModel.gameId() === "") {
            throw "encountered invalid condition in WaitingForMyWordState.joinGame: gameId is empty";
        }
        if (viewModel.myWord() === "") {
            throw "encountered invalid condition in WaitingForMyWordState.joinGame: myWord is empty";
        }
        apiUtils.updateGameWord2(viewModel.gameId(), viewModel.myWord());
    };
    WaitingForMyWordState.prototype.gameUpdatedWord2 = function () {
        console.log("WaitingForMyWordState.gameUpdatedWord2");
        viewModel.setGameState(new OpponentTurnState());
    };
    return WaitingForMyWordState;
}(StateBase));
var DoneState = (function (_super) {
    __extends(DoneState, _super);
    function DoneState() {
        var _this = _super.call(this) || this;
        console.log("DoneState");
        return _this;
    }
    Object.defineProperty(DoneState.prototype, "name", {
        get: function () { return "done"; },
        enumerable: true,
        configurable: true
    });
    DoneState.prototype.resetGame = function () {
        console.log("DoneState.resetGame");
        // game
        if (viewModel.isConnected()) {
            gameHub.server.leaveGame(viewModel.gameId());
        }
        viewModel.gameId("");
        viewModel.setGameState(new ChooseOpponentState());
        viewModel.guessToAdd("");
        viewModel.errorMessage("");
        viewModel.helps({ knownIn: "", knownOut: "", unknown: "", clues: [] });
        // me
        viewModel.myWord("");
        viewModel.myJotto(false);
        viewModel.myGuesses.removeAll();
        // opponent
        viewModel.opponentId("");
        viewModel.opponentIsComputer(false);
        viewModel.opponentJotto(false);
        viewModel.opponentGuesses.removeAll();
        // invitation
        viewModel.inviterPlayer({ id: "", name: "", isComputer: false });
        viewModel.invitedToGame({ id: "", player1Id: "", player2Id: "" });
    };
    return DoneState;
}(StateBase));
$(document).ready(function () {
    // globals
    viewModel = new ViewModel();
    apiUtils = new ApiUtils();
    ajaxUtils = new AjaxUtils();
    // SignalR stuff
    $.connection.hub.logging = true;
    $.connection.hub.disconnected(function () {
        viewModel.isConnected(false);
        new DoneState().resetGame();
        viewModel.myName("");
        viewModel.setGameState(new BeginState());
        viewModel.errorMessage("unable to maintain connection with server--resetting");
        $("#registerTxt").focus();
    });
    gameHub = $.connection.game;
    gameHub.client.gameStarted = function (game) {
        viewModel.gameStarted(game);
    };
    gameHub.client.turnTaken = function (guess) {
        viewModel.turnTaken(guess);
    };
    gameHub.client.gameUpdatedWord2 = function (game) {
        viewModel.gameUpdatedWord2(game);
    };
    gameHub.client.playerAdded = function (player) {
        if (player.id !== viewModel.myId()) {
            viewModel.playerAdded(player);
        }
    };
    $.connection.hub.start().done(function () {
        viewModel.isConnected(true);
        console.log("hub connection open");
    });
    // Knockout stuff
    ko.applyBindings(viewModel);
    // help the user
    $("#registerTxt").focus();
});
//# sourceMappingURL=jotto.js.map