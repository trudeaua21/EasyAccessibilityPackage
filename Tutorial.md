# Easy Accessibility Package Tutorial

__Author:__ Aaron Trudeau

__Special Thanks:__ Ira Woodring, Clinton "halfcoordinated" Lexa

__Last Updated__: April 2021

**Please note that this package is currently built only for use on Windows.**

## Contents
* [Introduction](#introduction)
* [Installation](#installation)
* [Remappable Controls](#remappable-controls)
* [Screen Reader Support](#screen-reader-support-using-tolk)

## Introduction
The Easy Accessibility Package is a Unity package containing code, prefabs, and assets to streamline the development of accessibility features. The purpose of this package is to make accessible game programming with Unity as easy as possible. While the package is tailored specifically towards smaller scale projects, projects of any scale can benefit from the features outlined within this package.

### Motivation
Accessibility in gaming is incredibly important - disabled gamers face situations where a lack of accessible design partially or completely locks them out of games far too often. Making your game accessible is crucial to allow as many people as possible to play it and enjoy it, and this package is meant to help you cut down on the time needed to achieve that goal.

### Features
Currently, this package includes two large main features:
* Remappable controls using the new Unity Input System
* Universal screen reader support using the [Tolk library](https://github.com/dkager/tolk) (courtesy of Davy Kager & other contributors)

### Uses
This package is designed to hopefully be useful in any context, but may be especially helpful if you are:
* Participating in a game jam and would like to add basic accessibility features 
* An indie developer looking to add accessibility features to their game
* A hobbyist dabbling in accessible game development

### Things to Note
* The Tolk Screen Reader Libraries are currently built for x86_64 (64-bit) architecture. If you would like to build your game in x86 (32-bit) architecture, the included libraries cannot be used (Tolk libraries can be built for 32-bit architecture, but those builds have not yet been included in the package). 
* If this package is being used on an existing project, you may not be able to make use of the remappable control features included in the project. This is because it uses the new [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Installation.html) rather than the built-in input system. The new Input System is much more versatile than the original and is much better for accessible programming. That being said, you can still use this package without using that feature - just be careful that you don't accidentally disable the old input system ([see Installation](#installation)).

**If you would like to switch to the new input system for an existing project, [this tutorial](https://www.raywenderlich.com/9671886-new-unity-input-system-getting-started) may prove helpful.**

## Installation 

1. Clone the repository, or otherwise download the `com.trudeaua.easyaccessibility` package folder to your computer.
2. In your Unity Project, open the Package Manager through **Window->Package Manager**.
3. Click the `+` dropdown icon at the top left of the Package manager window and select _Add package from disk_.
4. Navigate to the `com.trudeaua.easyaccessibility` package folder and select its `package.json`
5. If you don't have the `Input System` package installed already, a prompt will come up asking for you to enable the new input backends. Click **no** (we will enable them later).
6. Navigate to the `Packages/Easy Accessibility Package/Runtime/Screen Reader/Tolk DLLs` folder in the `Project` view.
7. Right click and select **Show In Explorer**.
8. Copy the files `nvdaControllerClient.dll`, `SAAPI64.dll`, and `Tolk.dll` (but _not_ any of the `.meta` files, or `TolkDotNet.dll`).
9. Return to the Unity Editor and navigate back to your `Assets` folder. Right click and select  **Show In Explorer**.
10. Navigate to the root directory of your project (where the `Assets` folder itself is located) and paste the copied DLL files.
11. Return to the Unity Editor.
12. Finally, navigate to **Edit->Project Settings->Player->Other Settings->Active Input Handling** and select **Both** from the dropdown menu. This will restart the Editor.
13. **Screen Reader Support in Game Builds** - The above instructions only provide screen reader support in the editor. To provide Screen Reader support in game builds, add the files `nvdaControllerClient.dll`, `SAAPI64.dll`, and `Tolk.dll` to your Build folder (or whatever the folder with your .exe is called). Thank you to [GrumpyCrouton](https://github.com/GrumpyCrouton) for [bringing this to my attention.](https://github.com/trudeaua21/EasyAccessibilityPackage/issues/24)

You're now ready to use the Easy Accessibility Package!

**This section is currently in a temporary state - the first 4 instructions will be different when the package is uploaded to Unity Asset Store.**

## Remappable Controls
Remappable controls are fairly simple to implement within the new Unity Input System. In fact, with the Input System package, Unity included an example scene that demonstrates how to do interactive rebinding of controls while the game is running. This tutorial is built on top of that example (and will not function correctly if you import the example into your project, as the classes within the included scripts would be defined twice.

The features included within this package that add to Unity's example include:
* Prefabs and Scripts that provide UI for inverting the X and Y axes on stick inputs (or any other Actions that take 2D-Vector inputs, such as Mouse Delta)
* An Input Processor to change "North" on a stick/Vector2 input (allowing you to set "up" to left on the stick, and rotate all other stick positions from there)
* A class that modifies Unity's `VirtualMouse` example to be driven by a mouse and allows for adjustable mouse sensitivity is included in the package but is currently unused due to a limitation of the `VirtualMouse` example that prevents it from working properly on scalable UI. [There is a fix that was being worked on at one point](https://github.com/Unity-Technologies/InputSystem/pull/1119), but it looks like it was abandoned - the class will work properly if the fix is ever integrated, but until then it will go unused. 

We'll demonstrate these features by adding this functionality to an example game that uses a _slightly_ modified version of the Unity interactive rebinding sample scene.

**Note that this is _not_ an Input System tutorial and assumes some level of familiarity with the Input System. That being said, it is still meant to be beginner friendly. If you've never used it before, seek out a tutorial or use this [quick start guide](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/QuickStartGuide.html) provided by Unity.**

### Setup
If you would like to follow along with this tutorial 1:1, follow these steps:
* To start, follow the installation instructions outlined in the [Installation](#installation) section.
* From there, open the Package Manager window by navigating to **Window->Package Manager**.
* Using the **Packages** dropdown (located to the right of the `+` dropdown), navigate to **Packages: In Project**.
* Navigate to the Easy Accessibility Package page and click on the **Samples** dropdown. Import the _Remappable Controls Example_ into the project by clicking the **Import** button. This will add a `Samples` folder to your assets folder.
* From the newly available `Samples/Easy Accessibility Package/[Version #]/Remappable Controls Example/Unfinished` folder, drag the `RebindGame` and `EAP-RebindingUISample` Scenes into the scene hierarchy, and remove the sample scene that Unity placed there on project creation (or any other scenes in the hierarchy). _If in Game view, everything is too zoomed in, switch the aspect ratio to Full HD._
* In the top editor menu, navigate to **File -> Build Settings...**. 
* At the bottom right of the top part of this menu that is labeled **Scenes In Build**, click **Add Open Scenes**. This will allow the scene manager to navigate between the two scenes.
* Exit from the **Build Settings...** menu and remove the `EAP-RebindingUISample` scene from the hierarchy.

### Explaining the "Game"
The "game" included with this example is very simple to start:
* Use W/A/S/D or Left Gamepad Stick to "Move"
* Use E or South Gamepad Button to "Interact"
* Use the Mouse or Right Gamepad Stick to "Look"

All these actions are displayed via text on the game screen.

Additionally, you get a quit button, and buttons to take you back and forth between a Settings screen (which starts out empty).

<figure>
    <figcaption><i>The main game screen of the example.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113737496-12edc800-96cc-11eb-9ae7-605548c45068.png" alt="TutorialImage" style="width:100%">
</figure>

<figure>
    <figcaption><i>The rebinding scene, which is a stripped-down version of an example included with Unity's Input System.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113739631-f9e61680-96cd-11eb-9c88-67c084c227d8.png" alt="TutorialImage" style="width:100%">
</figure>

### Adding Basic Control Remapping
Adding remappable controls to these actions is pretty simple, thanks to the example from Unity. Unity provides a prefab to allow interactive rebinding which I've lightly modified - this prefab is called `EAPRebindUIPrefab`. You can find this and other prefabs for this section by navigating to `Packages/Easy Accessibility Package/Runtime/Controls/Prefabs` in your `Project` window.

Open the scene `EAP-RebindingUISample`. You'll notice there are sections for the Keyboard and Mouse, as well as for the Gamepad controls. We'll start with adding rebindings to the Keyboard and Mouse.

#### Keyboard and Mouse

First, drag two `EAPRebindUIPrefab` items into your scene as children of the `Keyboard` GameObject on the `Canvas`. Rename them to `RebindMove` and `RebindInteract`.

<figure>
    <figcaption><i>The rebind scene after adding the prefabs.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113748267-446b9100-96d6-11eb-8d66-bd3c69edd3be.png" alt="TutorialImage" style="width:100%">
</figure>

To access the actions we'll be rebinding using these prefabs, click the expand icon on the Action Map Asset called `EAP-RebindUISampleActions` in the project view of the Unfinished Example folder. Find the `Interact` Action from this map and drag it to the `Action` argument on the `Rebind Action UI` component on `RebindInteract`. Next, on the `Binding` argument of the same component, select `E (Keyboard)`. Finally, find the `RebindOverlay` GameObject (which is a child of canvas), and view its children. Drag `RebindOverlay` to the `Rebind Overlay` argument on `RebindInteract` and drag its child `RebindPrompt` to the `Rebind Text` argument. 

It's fine to leave the `Display Options` argument as "Nothing" (pretty much anything is okay other than "Ignore Binding Overrides" - this will make it so your rebindings don't show up). 

Your scene should look something like this at this point:

<figure>
    <figcaption><i>The scene after finishing the rebind interact prefab.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113751632-230ca400-96da-11eb-9c49-deb02f3c3c97.png" alt="TutorialImage" style="width:100%">
</figure>

For the `Move` action, we'll follow a similar process, but with one small tweak. Just like before, drag the `Move` action to the appropriate spot on `RebindMove`. `Move` has a composite keyboard binding on it, meaning that it's split into 4 keys (WASD) for up/left/down/right. The prefab allows you to either rebind all parts of this composite at once or rebind each part one key at a time (with a prefab for each key). In this example, we'll rebind all of them at once - select the binding that includes all keys and set all the other arguments the same way you did for `Interact`.

Here's what your scene should look like now:

<figure>
    <figcaption><i>The scene after finishing both Keyboard rebind prefabs.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113752149-a4fccd00-96da-11eb-9174-4a7d7be09e16.png" alt="TutorialImage" style="width:100%">
</figure>


Now you can run and test. If you rebind controls and then go back to the game, you'll see that the controls you rebound to are now controlling the game!

#### Gamepad
The process for adding Gamepad rebindings will be pretty much the same as it was for the Keyboard rebindings. Drag 3 copies of the `EAPRebindUIPrefab` into the scene as children of the `Gamepad` GameObject on the `Canvas`. From top to bottom, name these `MoveRebind`, `LookRebind`, and `InteractRebind`. 

_Note that we choose to rebind `Look` here, but not for the Keyboard - this is because it makes more sense to be able to bind `Look` to either stick but doesn't make as much sense to rebind the Mouse input for `Look` interactively (this could be bound to button controls from a menu, from example._

<figure>
    <figcaption><i>The scene after adding the prefabs for the Gamepad section.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113753453-1a1cd200-96dc-11eb-8daa-baa9ced1a4bb.png" alt="TutorialImage" style="width:100%">
</figure>


Just as you did before, add the appropriate `Action` items from the `Project` view to these prefabs, choose the Gamepad bindings for each, and ensure that all the `Rebind Overlay` and `Rebind Text` arguments are set properly.

<figure>
    <figcaption><i>The scene after adding actions and bindings to the prefabs for the Gamepad section.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113756769-cad8a080-96df-11eb-9264-d24955c72f55.png" alt="TutorialImage" style="width:100%">
</figure>


At this point you can run the game and test it. However, you'll notice two problems:
* The binding names for the sticks aren't the most informative (being "RS" and "LS")
* The game crashes upon attempting a rebind

These two issues are part of the same problem. If you click on the `Gamepad` GameObject that's a child of `Canvas` (which acts as the container for our rebinding GameObjects), you'll notice it has a component on it called `Gamepad Icons Example`. This component's purpose is to switch the text display on the rebind buttons to icons instead. 

To make this happen (and to stop the errors), all you have to do is drag the `ActionBindingIcon` prefab (from the package's prefabs folder) into the scene as a child of the `TriggerRebindButton` GameObject on `MoveRebind`, `LookRebind`, and `InteractRebind`.

<figure>
    <figcaption><i>The scene after finishing all the prefabs for the Gamepad section.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113758385-cf05bd80-96e1-11eb-8386-c27969d1cc5b.png" alt="TutorialImage" style="width:100%">
</figure>

You can now run and test this. You'll notice that the icons pop up for each of the gamepad's actions, and that you can rebind them successfully (you can test this by going back to the game screen in play mode).

With that, the example game now has remappable controls!

**Note: By default, Unity's example supports mapping button inputs (such as `Interact`) to cardinal stick directions too - try it out!**

### Adding Save Functionality
In the work you have already done, the rebindings have actually saved each time that you've made any rebind! This is because of a modification I made to Unity's `RebindUIAction` prefab - each time anything is remapped, the rebind is saved to `PlayerPrefs`. 

That means the only thing left to make the rebindings persist between play sessions is to implement loading functionality. To do this, we only need to load the saved bindings once when the game starts and apply them to the Input Action Asset the game is using (the bindings persist between scenes automatically after this). 

To make this happen, simply open the `RebindGame` scene (which will be the main entry point to our game) and drag the `BindingLoader` prefab (located in `Packages/Easy Accessibility Package/Runtime/Controls/Prefabs` into the scene. Now, you can select the item in the scene, and drag the Input Action Asset you're using into the `Action Asset` argument. 

<figure>
    <figcaption><i>The game scene after adding the `BindingLoader`.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113763137-53a70a80-96e7-11eb-81bf-0feac4dcb574.png" alt="TutorialImage" style="width:100%">
</figure>

**NOTE: There's a chance that this actually may load some of the rebindings you've done in tests previously (since they saved automatically). It is possible that on your first time running, some controls may be bound to something you remapped them to in the past - these changes will show up on your settings screen when you run the game.**

To test whether the load worked without interference from this, run the game, remap a couple of controls, and stop running the game. Once you start the game again, the controls you remapped should have persisted! 

With that, you now have remappable controls that persist between play sessions!

### Adding Gamepad Stick Options
There are two main stick accessibility options included in this package at this point:
* Invert X/Y Axes
* Change Stick North

#### Invert X/Y Axes
This feature is easily implementable through the default [`InvertVector2` input processors](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Processors.html) that are included with the Input System. However, the package includes the UI for doing this with the rebind screen that we have already set up.

Inverting the X/Y axes of any stick or Vector2 input is as easy as adding the `StickInverterUI` prefab (from the package's Remappable Controls prefabs folder) as children of `MoveRebind` and `LookRebind`. This will add two toggles that apply the `InvertVector2` processor to that specific action (which saves automatically, just like our rebinds):

<figure>
    <figcaption><i>The settings scene after applying the <code>StickInverterUI</code> prefabs.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113764151-96b5ad80-96e8-11eb-9d80-927d3479f7e8.png" alt="TutorialImage" style="width:100%">
</figure>

You can now run and test. The axes you select to invert will now be inverted!

#### Change Stick North
The ability to change stick north (in other words, applying some rotation to all stick or Vector2 inputs) is useful for players who play more comfortably holding the controller at an angle (or sideways, or upside-down, etc.). Luckily, this package includes a custom input processor that makes this as easy to implement as X/Y axis inversion.

Similarly to the previous section, all you need to do is add the `StickNorthUI` prefab (from the package's Remappable Controls prefabs folder) as children of `MoveRebind` and `LookRebind`. This will add a dropdown menu that lets the player change stick north to a set of 8 main cardinal and ordinal directions (up, up-right, right, down-right, etc.). Whatever option is selected will be saved immediately, just like all other settings.

In the near future, this feature will be tweaked to still have this preset of directions but allow for north to be set to any other angle as well.

<figure>
    <figcaption><i>The settings scene after applying the <code>StickNorthUI</code> prefabs.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/113765830-c665b500-96ea-11eb-916d-6faa2bc1dacf.png" alt="TutorialImage" style="width:100%">
</figure>

You can now run and test. Stick north will change to whatever you set it to!

**Note that this _and_ axis inversion can be applied at the same time. To function properly, the Stick North Processor is always applied first, then the Axis Inversion Processor.**

### Where to go Next
From here, I recommend looking further into [`Processors` here](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Processors.html), as well as just getting familiar with the Input System in general. It still has a few irritating bugs (like not registering inputs from some controllers correctly), but after those are fixed, I'm confident that this will be the most accessible way to make Unity games!

Happy developing!

## Screen Reader Support Using Tolk
To make use of the included screen reader functionality within a scene, you first need to have the included `ScreenReader` prefab within the scene. To do this, simply drag the prefab from the package folder into the scene hierarchy. 

To use the screen reader from here, you have two main options:
* Use the static `ScreenReader` read methods from the `ScreenReader.cs` script to call read methods via script.
* Use Unity events to call read functions from the `ScreenReader` prefab.

You could use either or both of the above options - different options will be easier in different contexts. In general, 

* Calls to the static `ScreenReader` read methods from `ScreenReader.cs` will work well for using the Screen Reader in your own script.
* If you want to tie a screen reader output to a specific Unity Event (such as a button `OnClick` event) that can be changed through the editor, the non-static methods will work well.

From there, you can make most UI objects compatible with this `ScreenReader` object by adding a `ScreenReaderOutput` component.

With that base overview out of the way, let's get into how to include the screen reader functionality in your game!

### Adding a Screen Reader to an Existing Game
_To demonstrate how you could make your existing project accessible, this tutorial will cover how to make an existing sample project more accessible using the tools included with this package. However, the package also includes Button and Text prefabs that are already following all the best practices - if you want to make a new game, or build more UI for an existing project, these prefabs will come in handy._

This tutorial is based on the assets found in the folder `samples/ExistingGame-UNFINISHED`. If you want to see the final product we'll be making with this section, see the folder `samples/ExistingGame-FINISHED`.

The base project for this section is very simple - we have a two-scene game where the player clicks buttons to navigate between a main menu and the game screen. In the game screen, the user sees a Text object with a counter on it, a button to increment that counter, and a button to go back to the main menu. The "gameplay" is very simple but will show us a good number of best practices from both a UI _and_ gameplay perspective.

#### Setup
If you would like to follow along with this tutorial 1:1, follow these steps:
* To start, follow the installation instructions outlined in the [Installation](#installation) section.
* From there, open the Package Manager window by navigating to **Window->Package Manager**.
* Using the **Packages** dropdown (located to the right of the `+` dropdown), navigate to **Packages: In Project**.
* Navigate to the Easy Accessibility Package page and click on the **Samples** dropdown. Import the _Screen Reader Example_ into the project by clicking the **Import** button. This will add a `Samples` folder to your assets folder.
* From the newly available `Samples/Easy Accessibility Package/[Version #]/Screen Reader Example/Existing-Game-UNFINISHED` folder, drag the `SRGame` and `SRMainMenu` Scenes into the scene hierarchy, and remove the sample scene that Unity placed there on project creation (or any other scenes in the hierarchy).
* In the top editor menu, navigate to **File -> Build Settings...**. 
* At the bottom right of the top part of this menu that is labeled **Scenes In Build**, click **Add Open Scenes**. This will allow the scene manager to navigate between the two scenes.
* Exit from the **Build Settings...** menu and remove the `SRGame` scene from the hierarchy.

#### Accessible Main Menu UI
To start making this game screen reader accessible, we'll start with the main menu. 

The entire functionality of this part of the package relies on the `ScreenReader` prefab. This will be what allows our game to talk to the screen reader in several different ways. Drag this prefab into your hierarchy (you will need to do so for each scene you would like screen reader functionality in).

Per [this great source](https://uxdesign.cc/designing-main-menu-screens-for-visually-impaired-gamers-865a8bd76543) on making screen reader accessible menus, it's good to start each scene by outputting a description of the page you're on, as well as what controls the user has. The script is hard coded to output control information, so for each scene all you need to change is the `Scene Enter Message` field in the Screen Reader script component. Change this to "Main Menu Screen".

<figure>
    <figcaption><i>The scene after inserting the ScreenReader prefab.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/112406785-30458e00-8ceb-11eb-8489-eabb942abf5c.png" alt="TutorialImage" style="width:100%">
</figure>

If you test the scene now, the text you told the scene to read will be read! 

You'll notice a list on the Screen Reader script component as well. This is a list of `GameObjects` in our scene that we will want the user to be able to navigate between using the Tab key. Selected items will be read and highlighted.

However, before we can use this list, we need to have `GameObjects` in our scene that are compatible with our `ScreenReader`. To make any `GameObject` fit this bill, we can simply add a `ScreenReaderOutput` script component to it. Do this for both the `StartButton` and the `ExitButton` in the scene.

<figure>
    <figcaption><i>Screen Reader Output Script Component</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/112409286-9cc28c00-8cef-11eb-9460-1513a629271e.png" alt="TutorialImage" style="width:100%">
</figure>

The job of this component is to allow the `GameObject` it's attached to to read text when it's selected through the `ScreenReader` tab list, or when it's hovered over. Here's a description of each of the options on this component:

* `Read On Hover` - determines whether the text that the component will read will be read when the mouse hovers over the `GameObject` it's attached to.
* `Read from Text Object` - determines whether the text that the component will read should be read from a `Text` component that is attached to the `GameObject`.
* `Output Text` - This will only be visible/used if `Read from Text Object` is false. This is the text that the screen reader will read when the attached `GameObject` either is hovered over (if that option is selected) or selected in the `ScreenReader` list.

For our buttons, we will want to check read on hover, _not_ check reading from a text object, and set the output text to the name of the button ("Start Game Button" or "Exit Game button):

<figure>
    <figcaption><i>The Screen Reader Output component options for our Start Button</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/112409727-6a655e80-8cf0-11eb-83d2-90e449b1c5d8.png" alt="TutorialImage" style="width:100%">
</figure>

From here, all that's left is to drag and drop our `StartButton` and `ExitButton` game objects into the list on the `ScreenReader`. To do this, set the list's size to 2 and drag the buttons into the open slots:

<figure>
    <figcaption><i>The completed options for our Screen Reader script.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/112409910-ad273680-8cf0-11eb-9553-d7da3134d550.png" alt="TutorialImage" style="width:100%">
</figure>

If you run the game now, the outputs we coded in will be read when the buttons are hovered over or selected by pressing Tab. This will work for any other game object you use as well, so it's a very versatile system! Additionally, a button selected using Tab can be pressed by pressing the Enter/Return key. 

From here, we're actually only really missing one more key accessibility feature - output letting the user know when the button has been pressed. Luckily, this is very easy to do in our current system. 

On each of your Button's components, there should be an `OnClick()` section. To output text when this event occurs,

* Click the "+" at the bottom right of the list to add an extra entry.
* Under the "Runtime Only" dropdown, drag the `ScreenReader` game object that is in your scene (NOTE: there is a Unity bug where occasionally this won't work after modifying a script. If that occurs, just restart the Editor).
* From the "No Function dropdown, select **ScreenReader->ReadText(string)**
* In the text box that appears under the dropdown, add the text "\[Start/Exit\] button has been clicked."

<figure>
    <figcaption><i>The inspector window for the Button component on our StartButton.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/112410740-152a4c80-8cf2-11eb-9cb4-402dfcb480b4.png" alt="TutorialImage" style="width:100%">
</figure>

The screen reader will now read this text when the buttons are clicked!

#### Accessible Gameplay
We will now work on the "gameplay" portion of the example. To start, drag the `SRGame` into the hierarchy, and remove the `SRMainMenu` scene. 

<figure>
    <figcaption><i>The Game scene of the example.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/112567095-9cd98f00-8db6-11eb-92b3-b6febdf7cd83.png" alt="TutorialImage" style="width:100%">
</figure>

The only "gameplay" that the player has in this scene is to click the button to make the counter go up. 

To make this more accessible, we can start by adding the `ScreenReader` prefab to the scene, making a simple scene entry message, making the buttons accessible, and adding them to the list on the `ScreenReader`, just like we did in the previous step. Do so now to get some practice!

<figure>
    <figcaption><i>The Game scene after the buttons have been made accessible. The Scene entry message is "Game Screen. Click a button to add to the counter."</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/112568202-76b4ee80-8db8-11eb-95c5-66cd93a26296.png" alt="TutorialImage" style="width:100%">
</figure>

From here, the main drawback of the scene is that there is no accessible way to convey the counter information through the screen reader. This functionality is easily added to the screen by adding a `ScreenReaderOutput` component to the `CountText` object that displays the counter. In this case, since the main output of the `CountText` object is done via its `Text` component, we would check the `Read From Text Object` box on the component to make sure that the contents of our `Text` component are read, even after the text updates after the counter is clicked. Make sure the `Read On Hover` option is checked as well.

<figure>
    <figcaption><i>The `CountText` object has been made accessible through the `ScreenReaderOutput` component!</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/112568879-913b9780-8db9-11eb-8c0e-9ba7b671d742.png" alt="TutorialImage" style="width:100%">
</figure>

If you're wondering whether we should also add the `CountText` object to the tab-index list on the `ScreenReader`, your head is in the right place! However, in this case we won't - generally, it's best practice to only make interactable objects (like buttons, dropdown menus, scroll wheels, etc.) focusable through tab-indexing. 

That being said, there is one last thing we need to take care of before the game is fully accessible. For a moment, think about the count we're tracking as if it were the player's health. The count is actually stored on the `Player` object we have in the scene, so this metaphor is applicable in this example.

In gameplay, it's a good idea to allow the reading of player status items (such as health, ammo, etc.) to be mapped to a button input. We can do this through adding a control to our player script to read off our current count. Click the `Player` game object in the hierarchy, and then double-click the `Script` item on the `Player Controller (Script)` component in the inspector to open the `PlayerController` script in your code editor of choice.

In the `PlayerController` script, you can see that some basic input handling code for mapping an action to the `Space` key has been placed in the `Update` method:

<figure>
    <figcaption><i>The `Update` method of our `PlayerController` script.</i></figcaption>
    <img src="https://user-images.githubusercontent.com/36064197/112569727-18d5d600-8dbb-11eb-9ace-5cf25ef84bed.png" alt="TutorialImage" style="width:100%">
</figure>

So long as the `ScreenReader` prefab is in the scene that we want to programmatically read text from, outputting any text to the screen reader from a script is incredibly simple. All you need to do is call `ScreenReader.StaticReadText`, passing in the text you'd like to read. Here's how we'd do it in our example:

```
void Update()
{
    // map a button input to read a game status (the current count, in our case)
    if (Input.GetKeyDown(KeyCode.Space))
    {
        ScreenReader.StaticReadText("Count: " + count);
    }
}
```

Go back to the editor to run and test. Now, you should be able to read the count at any time by just pressing the `Space` key! Note that if you have another UI object focused at this point, that it will interact with that item when you press the `Space` key. This is intended - normally, during gameplay, you won't be clicking onscreen buttons, but rather using a controller or keyboard to get inputs, so this situation will likely not arise in your game.

#### Where to go Next
From here, I highly recommend you go to the tutorial I linked earlier [here](https://uxdesign.cc/designing-main-menu-screens-for-visually-impaired-gamers-865a8bd76543). It covers a great deal more about best practices for screen readers than I could go into for this tutorial. 

You should now be ready to use this package to make your game fully accessible to screen reader users! I hope this package makes the process and quick and easy as possible. 

Happy developing!

### Note for Experienced Developers 
The reason why I chose to make the `ScreenReader` prefab a requirement in each scene rather than making it fully static or a singleton is that I thought for newer developers, it would be easier to understand if all of the screen reader functionality was in one place. Because of this, I decided to include the static `ScreenReader` methods and the runtime functionality that allows for a list of GameObjects to be dragged and dropped straight into the `ScreenReader` prefab in one place. While it may have been better to decouple this functionality, I feel that for now this makes the concepts easier to understand for new developers. Additionally, if I made the static screen reader singleton a separate Game Object that persists throughout scenes, one would need to have two screen reader prefabs in their scene if they also wanted to use the runtime functionality! 
