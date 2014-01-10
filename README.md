RealChute
=========
RealChute was created and is maintained by stupid_chris. Message me on the forum if you have any questions.  
http://forum.kerbalspaceprogram.com/members/63758  

**Changelog**:  

*January 9th 2014*
**v0.3.3.2**
*Hotfix*
-Fixed the stack main chutes animation problem

*January 8th 2014*
**v0.3.3.1*
*Hotfix*
-Fixed a bug where single parachutes would still not arm propoerly
-Fixed stack main chute configs to have the right canopies so they actually work
-Fixed a problem with staging reset bugging the staging list
-Changed the default drag values of parts to 0.32 to actually match the real stock values
-Changed altitude detection to a faster, safer system
-Hopefully, the change above to altitude detection fixes parachutes deploying too early

*January 7th 2014*
**v0.3.3**
-Fixed a bug where dual chutes would not arm
-Changed default predeployment altitudes to 30km for drogues and 25km for mains
-Changed predeployment on drags to 100m and full deployment to 50m
-Switched triple canopies to single canopies on the stack 1.25m main chutes
-Fixed forced orientation so that it actually follows vessel orientation
-Fixed forced orientation remaining even if only one parachute is deployed.
-Small tweaks to attachement notes on stack chutes (thanks to eggrobin)
-Fixed caps being inverted on stack chutes (thanks to eggrobin again for making me notice)
-Changed behaviour or repacked chutes, if not in the last stage, they might not need to be moved to e reactivated.
-Moved the random deployment timer to OnStart() for future MechJeb implementation
-Various tweaks to the tweakables UI controllers
-Fixed a bug where dual chutes with the same material would show "empty" as a second material
-Fixed a bug where full deployment shortly after deployment would result in the animations skipping
-Fixed an annoying and unreliable bug where parachutes would make your craft spin out of control by making the force applied to the part once more

*December 18th 2013 (take two)*
**v0.3.2.1**
*Hotfix*
-Fixed the bug where dual parachutes would take mass forever
-Fixed a bug with combo chutes having ridiculous starting weight
-Finally fixed the bug with the FASA and Bargain Rocket parachutes, they will now animate properly (big thanks to sirkut)
-All the ModuleManager files are now included with the main download, remove those you don't want.

*December 18th 2013*
**v0.3.2**
*KSP 0.23 compatibility update!*
-Added combo chutes which contain both a drogue and a main for soft landings in one part
-Added the ability to define a second material for the second parachute
-Added the ability to force the parachutes partially in one direction, thus eliminating clipping chutes on dual parts!
-The force is now applied on the whole vessel, so no more weird hangings if the part origin is weird
-Tweakables! Nearly every value that can be changed in the editor can now be. This is only until I set the editor window up on my side.
-Fixed a bug with parachutes facing downwards on reentry
-Added a random deployment delay for parachutes! They will now take between 0 and 1 second to deploy. This is chose randomly for every parachute
-Every parachute now has random "movement noise" different from every other parachutes currently active
-Said random noise will now appear to be much smoother than before
-Usage of the new EFFECTS node has permited to get rid of FXGroups and to remove all those nasty .wav files all around!

*Decemer 7th 2013*  
**v0.3.1**  
*Hotfix*  
*Fixed a bug with the mustGoDown/timer clauses  
*Fixed a bug with dual chutes not cutting properly  
*Fixed yet another bug with predeployment of main chutes always having the same drag  
*Added an "reverseOrientation" clause in case a modeller builds the parachute transform the wrong way  
*Parachutes no create drag from the very area where they originate from. This means a chute on the side will hang   realistically without a CoM offset  
*Rescaled all the parachutes to have the real size in game. If you find this too big, tell me on the forum and I'll   revise them (note, this is not procedural yet, if you change the part yourself, you need to change the scale)  
*Fixed a bug with the calculator when calculating the diameter of multiple chutes  
*Added a sound on repack, thanks to ymir9 once again  

*December 4th 2013*  
**v0.3**  
*Completely removed stock drag dependancy. The parachutes now calculate drag according to real drag equations  
*Given the above, the drag parachutes generate is irrelevant of the mass of the part and now depends of the diameter of the parachute  
*Parachutes are now made of different materials, defined in cfg files  
*Parachute canopies now weight something which depends on which material they are made of and what is it's area density  
*Optimization of the code by about 300 lines as the module is pretty much complete  
*New parts with the pack, thanks to sumghai! You can now select between many different parachutes for the job you want to accomplish  
*It is now possible to arm a parachute to deploy as soon as it can through action group or part GUI  
*Minimal deployment can now be defined by altitude or pressure  
*Added FXGroups to parachute cut and repack, and providing a cut sound with the parachutes, offered by ymir9!  
*A small program made to help calculating parachute diameters is also included with the download!  

*November 24th 2013*  
**v0.2.1**  
*Hotfix*  
*Fixed a bug where single parachutes don't show the right icon colours  
*Fixed the weird glitchiness of the parachute's orientation in some occasions  
*Fixed parachutes not working if not on the current active vessel  

*November 23rd 2013*  
**v0.2**  
*Added a compatibility mode for a second parachute on the same part! Can only be used if the parachute has a
second set of parachute transform/animation  
*Reworked deployment code from the ground up to allow the above feature  

*November 16th 2013*   
**v0.1b**  
*Fixed the issue where deploying the parachute below the full deployment height would play the second animations faster  
*Added sounds on both predeployment and deployment  
*Added a deployment timer that will show a countdown on the flight screen  
*Added a clause that the ship must go downards to deploy and that will show status on the flight screen  
*Fixed a few remaining deployment bugs  
*Changed the behaviour of the part GUI to only show when the action is available  

*November 13th 2013*  
**v0.1.1a**  
*Hotfix of a few deployment bugs that could cause weird unresponsive parachutes  
*Added a "cutAlt" feature to automatically cut a parachute below a certain altitude.  

*November 12th 2013*  
**v0.1a**  
*Initial release  

---

RealChute.dll was made by stupid_chris  
The parts provided with the pack were all made by sumghai  
The parachute cut sound was made by ymir9  

Forum thread: http://forum.kerbalspaceprogram.com/threads/57988  
Development thread: http://forum.kerbalspaceprogram.com/threads/57987  

The plugin and parts are both licensed under CC-BY-NC-SA  
http://creativecommons.org/licenses/by-nc-sa/3.0/  
