# theta-pathfinding-unity
This is an example project that implements the Theta* (Theta star) pathfinding algorithm on Unity3D game engine.

The following resources were used in this project:
* The [Theta*](https://en.wikipedia.org/wiki/Theta*) algorithm in pseudocode.
* The [Line of Sight](https://news.movel.ai/theta-star) which was taken from this source.
* The [Priority Queue](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp) Not fully compatible so Dictionary also used to locate Node by Vector2
* The [FastNoise lite](https://github.com/Auburn/FastNoise) for map generation.
* [VContainer](vcontainer.hadashikick.jp/) for Dependency Injection.
* [Unitask](https://github.com/Cysharp/UniTask) for Model-View-ViewModel (MVVM) architecture (binding data model to UI components) and for handling async tasks.
* [MessagePipe](https://github.com/Cysharp/MessagePipe) for decoupling and sending messages from one object to another.
* [DoTween](https://dotween.demigiant.com/)

# Tests
To build a path between two points, click twice on the screen. 
Once the points are set, you can run a test that will perform 16 pathfinding tests between those same points. 
The results will be displayed in the log window.

If you want to terminate the process, click on the screen while the log animation is active. Note that pathfinding can be time-consuming and may lock the main thread.
![image](https://user-images.githubusercontent.com/1671030/225972409-bb031c2d-3743-4697-b2f2-7af2600d9a44.png)
