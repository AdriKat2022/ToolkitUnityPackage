# Utils Unity Package
## Description
I created this Unity package to help me in my various projects.  
I often stumbled upon repeating senarios where I needed a script for a specific mechanic but which is also often required when developing a game (a character controller, a menu system etc.).
Facing this "issue" of copying scripts from other projects (which is not really an issue but maybe a **threat** to the [DRY principle](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself) at a larger scale), I decided to learn how to make a Unity package!

## Content
Currently, there isn't much for the majority of games. But I intend to make this package grow in power and make it as much handy as possible.  
With care and patience, it will even be really useful for production!  

For the moment, here are the current contents of this package.

### Debugging
- **LiveLogger class**: Allows easy log display inspired by the UnrealEngine logging system. Offers a new log feedback layer by unclogging the Unity console for specific logs, or straight up if the Unity console is unavailable.

### Code Patterns
- **Singleton class**: A simple Singleton pattern implementation.
- **SingletonThreadSafe class**: A simple Singleton pattern implementation safer when using threading.

### Text Animations
*Various ready-to-use text animations.*  
*These scripts have a pretty much highly tweakable with many options serialized in the editor.*  
- **RainbowTextMeshPro class**: Highly tweakable rainbow effect.  

### Scripted Animations (UI & 3D)
*Various ready-to-use animations for UI components (anything that has a RectTransform) or for 3D objects.*  
- **ButtonAnimation class**: Script providing animations like scaling and rotating upon different button events. 
- **Camera Shake class**: Smart shaking effect that can be used on any Transform, usually the camera. Provides many options to tweak the shake effect.

### UI Elements
- **HoldAction class**: Script that allows to trigger an action when holding a button for a certain amount of time. Provides many options to tweak the holding visual effect.
- **SimpleTimer class**: Simple timer that can be used to display a count-up (and a countdown comming soon). Format is automated.

### Others
- **Mathematics class**: Various math utilities (currently only a method to map a value from a range to another).