# Easy Accessibility Package
The goal of the Easy Accessibility Package is to provide a free, easy to use resource to help Unity developers include accessibility features into their games. 

See the [tutorial](https://github.com/trudeaua21/EasyAccessibilityPackage/blob/main/Tutorial.md) for instructions on installation and use.

## Features 
There are two main features included with this package: **Remappable Controls** and **Screen Reader Support (using _Tolk_).**



### Remappable Controls
This feature includes:
* Modifications of Unity's _Input System_ examples to allow saving/loading remapped controls between play sessions.
* Scripts and `GameObjects` that allow stick directions to be remapped (both by inverting the X/Y axes, and by changing "North" on a joystick).
* Sample projects demonstrating the features listed above.

### Screen Reader Support (using _Tolk_)
This feature includes:
* The ability to interface with screen readers such as NVDA and JAWS in Unity (using the [_Tolk_ library](https://github.com/dkager/tolk)).
* Compiled 64-bit _Tolk_ DLLs needed to get _Tolk_ working within Unity (you can view _Tolk_'s source code and compile these yourself if you prefer).
* `GameObjects` that load the _Tolk_ library on load and provide a tab indexing system.
* Script components that make UI objects readable on hover and when included in the tab indexing system.



# Copyright Notice
## Easy Accessibility Package
This package is offered under the [MIT License](https://github.com/trudeaua21/EasyAccessibilityPackage/blob/main/LICENSE) 

## Tolk
_Tolk_ can be found at: [https://github.com/dkager/tolk](https://github.com/dkager/tolk)

To compile the _Tolk_ DLLs included in this package, _Tolk_'s Makefiles were modified to exclude compilation instructions for its Java libraries. None of the underlying source code that was compiled into the included DLLs was modified.

Even though I have packaged the _Tolk_ DLLs into this package, I still acknowledge that the more security-minded option is for you look at the source code and compile it yourself. However, I understand that adds a bit of a layer of difficulty to the usage of this package, so that may not be an option everyone pursues.

_Tolk_ is licensed under the GNU Lesser General Public License v3.0. See [Tolk-LICENSE](https://github.com/trudeaua21/EasyAccessibilityPackage/blob/main/Tolk-LICENSE)

## Input System
Unity's _Input System_ can be found at: [https://github.com/Unity-Technologies/InputSystem](https://github.com/Unity-Technologies/InputSystem)

Sample code and other assets (such as prefabs) provided with the _Input System_ have been modified and included within this package. 

Unity's _Input System_ is licensed under the Unity Companion Package License v1.0. [See its license here.](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/license/LICENSE.html)
