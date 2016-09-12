/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout/knockout.d.ts" />
/// <reference path="typings/signalr/signalr.d.ts" />

var viewModel: ViewModel;
var gameHub: HubProxy;
var ajaxUtils: AjaxUtils;
var apiUtils: ApiUtils;

interface SignalR {
    game: HubProxy;
}

interface HubProxy {
    client: IGameClient;
    server: IGameServer;
}

interface IGameServer {
    joinGame(gameId: string): void;
    leaveGame(gameId: string): void;
}

interface IGameClient {
    playerAdded(player: IJottoPlayer): void;
    gameUpdatedWord2(game: IJottoGame): void;
    gameStarted(game: IJottoGame): void;
    turnTaken(guess: ITurnTakenGuess): void;
}

interface IJottoGame {
    id: string;
    player1Id: string;
    player2Id: string;
}

interface IJottoPlayer {
    id: string;
    name: string;
    isComputer: boolean;
}

interface ITurnTakenGuess {
    gameId: string;
    playerId: string;
    word: string;
    score: number;
}

interface IGameHelper {
    knownIn: string;
    knownOut: string;
    unknown: string;
    clues: string[];
}

interface IHelperHint {
    playerId: string;
    letter: string;
    command: string;
}

interface IJottoState {
    name: string;
    registerPlayer(): void;
    chooseOpponent(player: IJottoPlayer): void;
    startGameAndWaitForOpponentWord(): void;
    gameStarted(game: IJottoGame): void;
    waitForOpponentToStart(): void;
    joinGame(): void;
    gameUpdatedWord2(): void;
    takeTurn(): void;
    turnTaken(guess: ITurnTakenGuess): void;
    supplyUnknownHint(hint: string): void;
    supplyKnownInHint(letter: string): void;
    supplyKnownOutHint(letter: string): void;
    resetHelps(): void;
    resetGame(): void;
}

class AjaxUtils {
    public getJson(url: string, success?: (data: any) => any, error?: () => void): void {
        $.ajax({
            type: "GET",
            url: url,
            success: success,
            error: error
        });
    }

    public postJson(url: string, data: any, success?: (data: any) => any, error?: (data: any) => any): JQueryXHR {
        return $.ajax({
            type: "POST",
            url: url,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(data),
            success: success,
            error: error
        });
    }

    public patchJson(url: string, data: any): JQueryXHR {
        return $.ajax({
            type: "PATCH",
            url: url,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(data)
        });
    }
}

class ApiUtils {
    public startGame(player1Id: string, player2Id: string, word1: string): void {
        ajaxUtils.postJson("api/games", { player1Id: player1Id, player2Id: player2Id, word1: word1 });
    }

    public registerPlayer(name: string, success: (player: IJottoPlayer) => any): void {
        ajaxUtils.getJson("api/players/" + name, success, () => {
            ajaxUtils.postJson("api/players", name, success);
        });
    }

    public getPlayer(playerId: string, success: (player: IJottoPlayer) => any): void {
        ajaxUtils.getJson("api/players/" + playerId, success);
    }

    public getPlayers(success: (players: Array<IJottoPlayer>) => any): void {
        ajaxUtils.getJson("api/players", success);
    }
    
    public makeGuess(gameId: string, playerId: string, forPlayerId: string, word: string,
                     success?: (data: any) => any, error?: (data: any) => any): void {
        ajaxUtils.postJson("api/games/" + gameId + "/guesses",
                           { playerId: playerId, forPlayerId: forPlayerId, guess: word }, success, error);
    }

    public updateGameWord2(gameId: string, word2: string): void {
        ajaxUtils.patchJson("api/games/" + gameId, word2);
    }

    public getGameHelps(gameId: string, success: (helps: IGameHelper[]) => any): void {
        ajaxUtils.getJson("api/games/" + gameId + "/helps", success);
    }

    public supplyHelperUnknownHint(gameId: string, playerId: string, letter: string, command: string,
                                   success: (data: any) => any, error?: (data: any) => any) {
        var hint: IHelperHint = { playerId: playerId, letter: letter, command: command };
        ajaxUtils.postJson("api/games/" + gameId + "/helps/unknown", hint, success, error);
    }

    public supplyHelperKnownInHint(gameId: string, playerId: string, letter: string,
                                   success: (data: any) => any, error?: (data: any) => any) {
        var hint: IHelperHint = { playerId: playerId, letter: letter, command: "" };
        ajaxUtils.postJson("api/games/" + gameId + "/helps/knownin", hint, success, error);
    }

    public supplyHelperKnownOutHint(gameId: string, playerId: string, letter: string,
                                    success: (data: any) => any, error?: (data: any) => any) {
        var hint: IHelperHint = { playerId: playerId, letter: letter, command: "" };
        ajaxUtils.postJson("api/games/" + gameId + "/helps/knownout", hint, success, error);
    }

    public resetHelps(gameId: string, playerId: string, success: (data: any) => any) {
        var hint: IHelperHint = { playerId: playerId, letter: "", command: "reset" };
        ajaxUtils.postJson("api/games/" + gameId + "/helps", hint, success);
    }
}

class ViewModel {
    constructor(
        public currentGameState: KnockoutObservable<IJottoState> = ko.observable<IJottoState>(),
        public gameState: KnockoutObservable<string> = ko.observable<string>(),
        public myId: KnockoutObservable<string> = ko.observable<string>(""),
        public myName: KnockoutObservable<string> = ko.observable<string>(""),
        public myWord: KnockoutObservable<string> = ko.observable<string>(""),
        public errorMessage: KnockoutObservable<string> = ko.observable<string>(""),
        public myGuesses: KnockoutObservableArray<ITurnTakenGuess> = ko.observableArray<ITurnTakenGuess>([]),
        public myJotto: KnockoutObservable<boolean> = ko.observable<boolean>(false),
        public opponents: KnockoutObservableArray<IJottoPlayer> = ko.observableArray<IJottoPlayer>([]),
        public gameId: KnockoutObservable<string> = ko.observable<string>(""),
        public inviterPlayer: KnockoutObservable<IJottoPlayer> = ko.observable<IJottoPlayer>({ id: "", name: "", isComputer: false }),
        public invitedToGame: KnockoutObservable<IJottoGame> = ko.observable<IJottoGame>({ id: "", player1Id: "", player2Id: ""}),
        public opponentId: KnockoutObservable<string> = ko.observable<string>(""),
        public opponentIsComputer: KnockoutObservable<boolean> = ko.observable<boolean>(false),
        public opponentGuesses: KnockoutObservableArray<ITurnTakenGuess> = ko.observableArray<ITurnTakenGuess>([]),
        public opponentJotto: KnockoutObservable<boolean> = ko.observable<boolean>(false),
        public guessToAdd: KnockoutObservable<string> = ko.observable<string>(""),
        public playerToRegister: KnockoutObservable<string> = ko.observable<string>(""),
        public helps: KnockoutObservable<IGameHelper> = ko.observable<IGameHelper>({ knownIn: "", knownOut: "", unknown: "", clues: [] }),
        public isConnected: KnockoutObservable<boolean> = ko.observable(false)) {

        this.setGameState(new BeginState());
    }

    public setGameState(state: IJottoState): void {
        this.currentGameState(state);
        this.gameState(state.name);
    }

    public playerAdded(player: IJottoPlayer): void {
        this.opponents.push(player);
    }

    public gameUpdatedWord2(game: IJottoGame): void {
        if (this.gameId() === game.id) {
            gameHub.server.joinGame(game.id);  // subscribe for notifications about my game
            this.currentGameState().gameUpdatedWord2();
        }
    }

    public gameStarted(game: IJottoGame) {
        if (this.myId() === "") {
            this.invitedToGame(game);   // maybe it's for me after I register
            return;
        }
        if (game.player1Id !== viewModel.myId() && game.player2Id !== viewModel.myId()) {
            return; // don't care about other folks' games
        }
        if (this.gameId() !== "") {
            return; // I'm already in a game, so don't join a new one
        }

        gameHub.server.joinGame(game.id);  // subscribe for notifications about my game

        this.gameId(game.id);
        this.currentGameState().gameStarted(game);

        if (this.gameState() === "myTurn") {
            $("#guessTxt").focus();
        }
    }

    public turnTaken(guess: ITurnTakenGuess): void {
        if (guess.playerId === this.myId()) {
            this.myGuesses.push(guess);
            if (guess.score === 6) {
                this.myJotto(true);
            }
        } else {
            this.opponentGuesses.push(guess);
            if (guess.score === 6) {
                this.opponentJotto(true);
            }
        }
        this.currentGameState().turnTaken(guess);

        if (this.gameState() === "myTurn") {
            $("#guessTxt").focus();
        }
    }
}

class StateBase implements IJottoState {
    constructor() {
        //
    }
    public get name(): string { throw "not implemented"; }
    public registerPlayer(): void { throw "registerPlayer not implemented"; }
    public chooseOpponent(player: IJottoPlayer): void { throw "chooseOpponent not implemented"; }
    public startGameAndWaitForOpponentWord(): void { throw "startGameAndWaitForOpponentWord not implemented"; }
    public waitForOpponentToStart(): void { throw "waitForOpponentToStart not implemented"; }
    public gameStarted(game: IJottoGame): void { throw "gameStarted not implemented"; }
    public joinGame(): void { throw "joinGame not implemented"; }
    public gameUpdatedWord2(): void { throw "gameUpdatedWord2 not implemented"; }
    public takeTurn(): void { throw "takeTurn not implemented"; }
    public turnTaken(guess: ITurnTakenGuess): void { throw "turnTaken not implemented"; }
    public supplyUnknownHint(hint: string): void { throw "supplyUnknownHint not implemented"; }
    public supplyKnownInHint(letter: string): void { throw "supplyKnownInHint not implemented"; }
    public supplyKnownOutHint(letter: string): void { throw "supplyKnownOutHint not implemented"; }
    public resetHelps(): void { throw "resetHelps not implemented"; }
    public resetGame(): void { throw "resetGame not implemented"; }
}

class BeginState extends StateBase {
    constructor() {
        super();
        console.log("BeginState");
    }

    public get name(): string { return "begin"; }

    public registerPlayer(): void {
        console.log("BeginState.registerPlayer");

        if (!viewModel.isConnected()) {
            $.connection.hub.start();
        }

        if (viewModel.playerToRegister() !== "") {
            viewModel.errorMessage("");
            var playerName = viewModel.playerToRegister();
            apiUtils.registerPlayer(playerName, (player: IJottoPlayer) => {
                viewModel.myId(player.id);
                viewModel.myName(player.name);
                if (viewModel.invitedToGame().player2Id === viewModel.myId()) {
                    viewModel.gameId(viewModel.invitedToGame().id);
                    apiUtils.getPlayer(viewModel.invitedToGame().player1Id, (playerById: IJottoPlayer) => {
                        viewModel.inviterPlayer(playerById);
                    });
                    viewModel.invitedToGame({ id: "", player1Id: "", player2Id: ""});
                }
                apiUtils.getPlayers((players: Array<IJottoPlayer>) => {
                    viewModel.opponents(players.filter((playerToFilter: IJottoPlayer)=> {
                         return playerToFilter.id !== viewModel.myId();
                    }));
                    viewModel.setGameState(new ChooseOpponentState());
                });
            });
            viewModel.playerToRegister("");
        }
    }
}

class ChooseOpponentState extends StateBase {
    constructor() {
        super();
        console.log("ChooseOpponentState");
    }

    get name(): string { return "chooseOpponent"; }

    public chooseOpponent(player: IJottoPlayer): void {
        console.log("ChooseOpponentState.chooseOpponent");

        viewModel.opponentId(player.id);
        if (player.isComputer) {
            viewModel.opponentIsComputer(true);
            apiUtils.startGame(viewModel.myId(), viewModel.opponentId(), viewModel.myWord());
        } else {
            viewModel.setGameState(new ChooseWordState());
            $("#wordTxt").focus();
        }
    }

    public gameStarted(game: IJottoGame): void {
        console.log("ChooseOpponentState.gameStarted");

        if (viewModel.opponentIsComputer()) {
            viewModel.setGameState(new MyTurnState());
        } else {
            apiUtils.getPlayer(game.player1Id, (player: IJottoPlayer) => {
                viewModel.inviterPlayer(player);
                viewModel.setGameState(new InvitationPendingState());
            });
        }
    }
}

class InvitationPendingState extends StateBase {
    constructor() {
        super();
        console.log("InvitationPendingState");
    }

    get name(): string { return "invitationPending"; }

    public chooseOpponent(player: IJottoPlayer): void {
        console.log("InvitationPendingState.chooseOpponent");

        viewModel.opponentId(player.id);
        viewModel.setGameState(new ChooseWordState());
        $("#wordTxt").focus();
    }
}

class ChooseWordState extends StateBase {
    constructor() {
        super();
        console.log("ChooseWordState");
    }

    get name(): string { return "chooseWord"; }

    public startGameAndWaitForOpponentWord(): void {
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
    }

    public waitForOpponentToStart(): void {
        console.log("ChooseWordState.waitForOpponentToStart");

        if (viewModel.myWord() === "") {
            throw "encountered invalid condition in ChooseWordState.waitForOpponentToStart: myWord is empty";
        }

        if (viewModel.gameId() === "") {
            viewModel.setGameState(new WaitingForOpponentToStartState());
        } else {
            apiUtils.updateGameWord2(viewModel.gameId(), viewModel.myWord());
        }
    }

    public gameStarted(game: IJottoGame): void {
        console.log("ChooseWordState.gameStarted");

        viewModel.setGameState(new WaitingForMyWordState());
    }

    public joinGame(): void {
        console.log("ChooseWordState.joinGame");

        if (viewModel.gameId() === "") {
            throw "encountered invalid condition in WaitingForMyWordState.joinGame: gameId is empty";
        }
        if (viewModel.myWord() === "") {
            throw "encountered invalid condition in WaitingForMyWordState.joinGame: myWord is empty";
        }
        gameHub.server.joinGame(viewModel.gameId());  // subscribe for notifications about my game
        apiUtils.updateGameWord2(viewModel.gameId(), viewModel.myWord());
    }

    public gameUpdatedWord2(): void {
        console.log("ChooseWordState.gameUpdatedWord2");

        viewModel.setGameState(new OpponentTurnState());
    }
}

class WaitingForOpponentToStartState extends StateBase {
    constructor() {
        super();
        console.log("WaitingForOpponentToStartState");
    }

    get name(): string { return "waitingForOpponentToStart"; }

    public gameStarted(game: IJottoGame): void {
        console.log("WaitingForOpponentToStartState.gameStarted");

        if (game.id === "") {
            throw "encountered invalid condition in ChooseWordState.waitForOpponentToStart: game id is empty";
        }
        if (viewModel.myWord() === "") {
            throw "encountered invalid condition in ChooseWordState.waitForOpponentToStart: myWord is empty";
        }

        apiUtils.updateGameWord2(viewModel.gameId(), viewModel.myWord());
    }

    public gameUpdatedWord2(): void {
        console.log("WaitingForOpponentToStartState.gameUpdatedWord2");

        // let's try staying in this state and only switching to a new state once they've taken their turn
        //viewModel.setGameState(new OpponentTurnState());
    }

    public turnTaken(): void {
        console.log("WaitingForOpponentToStartState.turnTaken");

        viewModel.setGameState(new MyTurnState());
    }
}

class WaitingForOpponentWordState extends StateBase {
    constructor() {
        super();
        console.log("WaitingForOpponentWordState");
    }

    get name(): string { return "waitingForOpponentWord"; }

    public gameUpdatedWord2(): void {
        console.log("WaitingForOpponentWordState.gameUpdatedWord2");

        viewModel.setGameState(new MyTurnState());
    }

    public gameStarted(game: IJottoGame): void {
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
    }
}

class MyTurnState extends StateBase {
    constructor() {
        super();
        console.log("MyTurnState");
    }

    get name(): string { return "myTurn"; }

    public takeTurn(): void {
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
            apiUtils.makeGuess(viewModel.gameId(), viewModel.myId(), viewModel.opponentId(), viewModel.guessToAdd(),
                () => { // success
                    viewModel.errorMessage("");
                    apiUtils.getGameHelps(viewModel.gameId(), (helps: IGameHelper[]) => {
                        if (helps.length > 0) {
                            viewModel.helps(helps[0]);
                        }
                    });
                },
                (data) => { // error
                    var responseText: string = JSON.parse(JSON.stringify(data)).responseText;
                    viewModel.errorMessage(JSON.parse(responseText).message);
                });
            viewModel.guessToAdd("");
        }
    }

    public turnTaken(guess: ITurnTakenGuess): void {
        console.log("MyTurnState.turnTaken");

        if (viewModel.opponentIsComputer()) {
            if (viewModel.myJotto()) {
                viewModel.setGameState(new DoneState());
            }
            // else just stay in MyTurnState
        } else {
            // since we're in MyTurnState, normally it would be their turn
            if (viewModel.opponentJotto()) {
                if (viewModel.myJotto()) {
                    // we've both got jotto, so we're done
                    viewModel.setGameState(new DoneState());
                }
                // else it would normally be their turn, but they've got jotto, so stay in MyTurnState
            } else {
                // normal state of affairs
                viewModel.setGameState(new OpponentTurnState());
            }
        }
    }

    public gameUpdatedWord2(): void {
        console.log("MyTurnState.gameUpdatedWord2");

        // no-op: timing may cause this state to be entered prior to opponent providing their word
        // not sure if this is a problem yet.
    }

    public gameStarted(game: IJottoGame): void {
        console.log("MyTurnState.gameStarted");

        // no-op: ignore, 'cause I'm already in the middle of a game
    }

    public supplyUnknownHint(hint: string): void {
        // hint comes in as letter followed by +/-, e.g. "e+", "q-", etc.
        console.log(hint);
        apiUtils.supplyHelperUnknownHint(viewModel.gameId(), viewModel.myId(), hint.substr(0, 1), hint.substr(1, 1),
            () => {  // success
                apiUtils.getGameHelps(viewModel.gameId(), (helps) => {
                    viewModel.errorMessage("");
                    viewModel.helps(helps[0]);
                    $("#guessTxt").focus();
                });
            },
            (data) => {  // error
                var responseText: string = JSON.parse(JSON.stringify(data)).responseText;
                viewModel.errorMessage(JSON.parse(responseText).message);
                $("#guessTxt").focus();
            });
    }

    public supplyKnownInHint(letter: string): void {
        console.log(letter);
        apiUtils.supplyHelperKnownInHint(viewModel.gameId(), viewModel.myId(), letter, () => {  // success
            apiUtils.getGameHelps(viewModel.gameId(), (helps) => {
                viewModel.errorMessage("");
                viewModel.helps(helps[0]);
                $("#guessTxt").focus();
            });
        }, (data) => {  // error
            var responseText: string = JSON.parse(JSON.stringify(data)).responseText;
            viewModel.errorMessage(JSON.parse(responseText).message);
            $("#guessTxt").focus();
        });
    }

    public supplyKnownOutHint(letter: string): void {
        console.log(letter);
        apiUtils.supplyHelperKnownOutHint(viewModel.gameId(), viewModel.myId(), letter, () => { // success
            apiUtils.getGameHelps(viewModel.gameId(), (helps) => {
                viewModel.errorMessage("");
                viewModel.helps(helps[0]);
                $("#guessTxt").focus();
            });
        }, (data) => {  // error
            var responseText: string = JSON.parse(JSON.stringify(data)).responseText;
            viewModel.errorMessage(JSON.parse(responseText).message);
            $("#guessTxt").focus();
        });
    }

    public resetHelps(): void {
        apiUtils.resetHelps(viewModel.gameId(), viewModel.myId(), () => {   // success
            apiUtils.getGameHelps(viewModel.gameId(), (helps) => {
                viewModel.errorMessage("");
                viewModel.helps(helps[0]);
                $("#guessTxt").focus();
            });
        });
    }
}

class OpponentTurnState extends StateBase {
    constructor() {
        super();
        console.log("OpponentTurnState");
    }

    get name(): string { return "opponentTurn"; }

    public turnTaken(guess: ITurnTakenGuess): void {
        console.log("OpponentTurnState.turnTaken");

        if (viewModel.opponentIsComputer()) {
            throw "encountered invalid condition in OpponentTurnState.turnTaken: opponent can't be a computer";
        } else {
            // since we're in OpponentTurnState, normally it would be my turn
            if (viewModel.myJotto()) {
                if (viewModel.opponentJotto()) {
                    // we've both got jotto, so we're done
                    viewModel.setGameState(new DoneState());
                }
                // else it would normally be my turn, but I've got jotto, so stay in OpponentTurnState
            } else {
                // normal state of affairs
                viewModel.setGameState(new MyTurnState());
            }
        }
    }

    public gameStarted(game: IJottoGame): void {
        console.log("OpponentTurnState.gameStarted");

        // no-op: ignore, 'cause I'm already in the middle of a game
    }
}

class WaitingForMyWordState extends StateBase {
    constructor() {
        super();
        console.log("WaitingForMyWordState");
    }

    get name(): string { return "waitingForMyWord"; }

    public joinGame(): void {
        console.log("WaitingForMyWordState.joinGame");

        if (viewModel.gameId() === "") {
            throw "encountered invalid condition in WaitingForMyWordState.joinGame: gameId is empty";
        }
        if (viewModel.myWord() === "") {
            throw "encountered invalid condition in WaitingForMyWordState.joinGame: myWord is empty";
        }
        apiUtils.updateGameWord2(viewModel.gameId(), viewModel.myWord());
    }

    public gameUpdatedWord2(): void {
        console.log("WaitingForMyWordState.gameUpdatedWord2");

        viewModel.setGameState(new OpponentTurnState());
    }
}

class DoneState extends StateBase {
    constructor() {
        super();
        console.log("DoneState");
    }

    get name(): string { return "done"; }

    public resetGame(): void {
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
    }
}

$(document).ready(() => {
    // globals
    viewModel = new ViewModel();
    apiUtils = new ApiUtils();
    ajaxUtils = new AjaxUtils();

    // SignalR stuff
    $.connection.hub.logging = true;
    $.connection.hub.disconnected(() => {
        viewModel.isConnected(false);
        new DoneState().resetGame();
        viewModel.myName("");
        viewModel.setGameState(new BeginState());
        viewModel.errorMessage("unable to maintain connection with server--resetting");
        $("#registerTxt").focus();
    });
    gameHub = $.connection.game;

    gameHub.client.gameStarted = (game: IJottoGame) => {
        viewModel.gameStarted(game);
    };

    gameHub.client.turnTaken = (guess: ITurnTakenGuess) => {
        viewModel.turnTaken(guess);
    };

    gameHub.client.gameUpdatedWord2 = (game: IJottoGame) => {
        viewModel.gameUpdatedWord2(game);
    };

    gameHub.client.playerAdded = (player: IJottoPlayer) => {
        if (player.id !== viewModel.myId()) {
            viewModel.playerAdded(player);
        }
    };

    $.connection.hub.start().done(() => {
        viewModel.isConnected(true);
        console.log("hub connection open");
    });

    // Knockout stuff
    ko.applyBindings(viewModel);

    // help the user
    $("#registerTxt").focus();
});
