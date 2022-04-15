
These changes to game creator make it possible to define actions to be executed when an animation event occurs.

To setup - 

Add `AnimationEffectSpawn` component to the "Character" component (the one with the animator) see effect_spawn.png

In your animation clips - create an event calling function "InvokeAction" the string parameter is the name of your specific event - the exact name "InvokeAction" is important! see event_definition.png

Once setup properly the string parameter of your animation event will appear in the melee clip while inspecting! see melee_clip.png