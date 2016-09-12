## Synopsis

A browser-based version of [Jotto](https://en.wikipedia.org/wiki/Jotto) that supports human v. computer and human v. human games.

## Motivation

This project is intended as a playground for an ASP.NET- and SignalR-based version of the game.  The UI is intentionally quick-n-dirty, with emphasis only on functionality and none on aesthetics.

## Play the Game

A running instance of this game can be found [here](http://jotto.seanhokanson.org/), hosted on Azure.

## Issues

More than can be enumerated, but here are some that need work, in no particular order:
* Using Enter key vs. button click to send the Guess is screwy on some browsers
* Need to rationalize state management somewhat
	* Use the response from an API call to handle state changes?
	* Use the SignalR eventing to handle state changes?  This is the current paradigm.
* Probably some bugs in the state management for setting up a two-player game.  Once the game has started, should be OK.
* Absolutely no idea about performance/stability under load (i.e. with lots of games in progress)