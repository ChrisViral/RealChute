RealChute
=========
RealChute was created and is maintained by stupid_chris. Message me on the forum if you have any questions.
http://forum.kerbalspaceprogram.com/members/63758

Changelog:
Decemer 7th 2013
v0.3.1
*Hotfix*
-Fixed a bug with the mustGoDown/timer clauses
-Fixed a bug with dual chutes not cutting properly
-Fixed yet another bug with predeployment of main chutes always having the same drag
-Added an "reverseOrientation" clause in case a modeller builds the parachute transform the wrong way
-Parachutes no create drag from the very area where they originate from. This means a chute on the side will hang realistically without a CoM offset
-Rescaled all the parachutes to have the real size in game. If you find this too big, tell me on the forum and I'll revise them (note, this is not procedural yet, if you change the part yourself, you need to change the scale)
-Fixed a bug with the calculator when calculating the diameter of multiple chutes
-Added a sound on repack, thanks to ymir9 once again

December 4th 2013
v0.3
-Completely removed stock drag dependancy. The parachutes now calculate drag according to real drag equations
-Given the above, the drag parachutes generate is irrelevant of the mass of the part and now depends of the diameter of the parachute
-Parachutes are now made of different materials, defined in cfg files
-Parachute canopies now weight something which depends on which material they are made of and what is it's area density
-Optimization of the code by about 300 lines as the module is pretty much complete
-New parts with the pack, thanks to sumghai! You can now select between many different parachutes for the job you want to accomplish
-It is now possible to arm a parachute to deploy as soon as it can through action group or part GUI
-Minimal deployment can now be defined by altitude or pressure
-Added FXGroups to parachute cut and repack, and providing a cut sound with the parachutes, offered by ymir9!
-A small program made to help calculating parachute diameters is also included with the download!

November 24th 2013
v0.2.1
*Hotfix*
-Fixed a bug where single parachutes don't show the right icon colours
-Fixed the weird glitchiness of the parachute's orientation in some occasions
-Fixed parachutes not working if not on the current active vessel

November 23rd 2013
v0.2
-Added a compatibility mode for a second parachute on the same part! Can only be used if the parachute has a a
second set of parachute transform/animation
-Reworked deployment code from the ground up to allow the above feature

November 16th 2013
v0.1b
-Fixed the issue where deploying the parachute below the full deployment height would play the second animations faster
-Added sounds on both predeployment and deployment
-Added a deployment timer that will show a countdown on the flight screen
-Added a clause that the ship must go downards to deploy and that will show status on the flight screen
-Fixed a few remaining deployment bugs
-Changed the behaviour of the part GUI to only show when the action is available

November 13th 2013
v0.1.1a
-Hotfix of a few deployment bugs that could cause weird unresponsive parachutes
-Added a "cutAlt" feature to automatically cut a parachute below a certain altitude.


November 12th 2013
v0.1a
-Initial release

This work is licensed under CC-BY-NC-SA
http://creativecommons.org/licenses/by-nc-sa/3.0/
