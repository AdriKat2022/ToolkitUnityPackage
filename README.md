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
- **Text Animation Scripts**: Various ready-to-use text animations, including a rainbow effect only for the moment. These scripts have a pretty much highly tweakable with many options serialized in the editor.  
- **UI Element Animation Scripts**: Various ready-to-use animations for UI components (anything that has a RectTransform). Including, a button animation script.  
- **Camera Shake class**: Smart shaking effect that can be used on any Transform, usually the camera. Provides many options to make it as you prefer!
