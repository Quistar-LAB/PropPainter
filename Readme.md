# Prop Painter: Revisited 
### __(A replacement for the aging Prop Painter)__
<a href="LICENSE">
	<img src="https://img.shields.io/badge/license-MIT-green" />
</a>

#### Requires: Harmony 2.0.x ported over to Cities Skylines by [Bofomer](https://github.com/boformer) -- [Github](https://github.com/boformer/CitiesHarmony)<br>Requires: MoveIt by [Quboid](https://github.com/Quboid) -- [Github](https://github.com/Quboid/CS-MoveIt)
Prop Painter: Revisited is a re-write of the original Prop Painter mod with fixes and performance improvements.

Improvements compared to the old Prop Painter includes:
(1) Less memory footprint, as no dictionary or lists are used. One simple array is used to store all 65k props colors.
(2) Loading and saving is faster, as no extra memory is created to save/load color data.
(3) Faster color access compared to old Prop Painter, as all color access is done within a delegate with color array stored in the stack.
(4) Only one method is patched compared to old Prop Painter patching 4 methods (of which 3 were patching MoveIt methods)
(5) Does not create any unnecessary gameobjects, contrary to the old Prop Painter which creates one GameObject to run color data synchronization

This mod requires:
- MoveIt mod
- Harmony

I need supporters/volunteers to help debug/code to make this mod even better. If you want to contribute, please contact me anytime.

Anyways, these codes are open to the public, as its a hobby of mine. If you wish to contribute to the codes, please join in.

IMPORTANT!! As always, create a new save!!! This mod creates a new version of saved datas. Original mod formats loading are supported, but then are saved into the new format.

