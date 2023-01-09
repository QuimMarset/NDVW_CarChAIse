# CarChAIse
This repository contains the Unity Engine project for the Car ChAIse videogame developed for the Normative Dynamic Virtual Worlds (NDVW) course of the [Master in Artificial Intelligence](https://masters.fib.upc.edu/masters/master-artificial-intelligence) from UPC, UB and URV. 
Car ChAIse is an endless racing game where the player assumes the role of a driver pursued by computer-controlled police cars along the roads full of civilians of a city. The player's goal is to reach as many destinations as possible, while trying to escape the police and mitigate any damage to the car. Destinations are random points of the city that are endlessly generated. Every time that the player reaches one, his score is incremented and another destination appears. The police tries to chase the player or destroy its car, while avoiding the civilian cars. Police agents communicate between them the last player known location and start to patrol if the player is lost. Civilians, on the other hand, go on about their lives driving around the city, trying not to complicate the task of the police. The game's difficulty increases depending on player's score, incrementing the amount of police cars. Therefore, the player is assumed to be caught by the police or get his car destroyed after an amount of time, finishing the playthrough and showing the obtained score. It is expected that the player want to repeat the game and improve that score. The city is procedurally generated on each playthrough, creating new buildings and streets.

Authors:
* Cristian Andrés Camargo Giraldo
* Demetre Dzmanashvili
* Benet Manzanares Salor
* Joaquim Marset Alsina

## How to install
Create a default 3D Unity project (only tested with Unity 2021.3) and copy the files contained in this repository. The MainMenu scene in 'Assets/OurAssets/Scenes'' allows to start the game.