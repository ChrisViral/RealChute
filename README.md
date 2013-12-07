To install RealChute, simply merge the included GameData folder with the one in your KSP directory.

Components of the module:
--Sounds-- (the name of the sound goes in front)
sound_name_1 = rcpredeploy --> sound to play on predeployment of the main chute
sound_name_2 = rcdeploy --> sound to play on full deployment of the main chute
sound_name_3 = rcsecpredeploy --> sound to play on full deployment of the secondary chute
sound_name_4 = rcsecdeploy --> sound to play on full deployment of the secondary chute
sound_name_5 = rccut --> sound to play when the parachute is cut
sound_name_5 = rcrepack --> sound to play when the parachute is repacked

--Part Module--
//General
material: what material the parachute is made out (must be initiated in a MATERIAL{} node)
caseMass: mass of the case of the parachute (what you would normally put at "mass = xx" (put it there too though))
timer: time before deployment after the part has been activated in seconds
mustGoDown: whether or not the craft has to be going downwards to deploy (true/false)
cutSpeed: speed at which the parachute will automatically cut when on the ground in m/s
spareChutes: amount of times a parachute can be repacked (set to -1 if you want to repack it as much as you want)
reverseOrientation: check this to true if the transform was built backwards, else don't put this ine at all
secondaryChute: if the part uses two parachutes (true/false, the part needs double the elements from the model, no need to include this line if there's no secondary chute)

//Main parachute
preDeployedDiameter: diameter of the parachute when predeployed
deployedDiameter: diameter of the parachute when deployed
minIsPressure: whether the value in "minDeployment" is pressure or altitude (true/false)
minDeployment: minimum altitude or pressure at which the parachute can be opened
deploymentAlt: altitude at which the parachute will fully deploy
cutAlt: altitude at which the parachute will automatically cut (set this to -1 if you don't want an autocut at a certain altitude)
preDeploymentSpeed: time required to predeploy the parachute in seconds (affects how fast it decelerates)
deploymentSpeed: time required to deploy the parachute in seconds (affects how fast it decelerates)
preDeploymentAnimation: name of the predeployment animation (from the model)
deploymentAnimation: name of the deployment animation (from the model)
parachuteName: name of the canopy of the parachute (from the model)
capName: name of the protective cap of the parachute (from the model)

//Secondary parachute
secPreDeployedDiameter: diameter of the second parachute when predeployed
secDeployedDiameter: diameter of the second parachute when deployed
secMinIsPressure: whether the value in "secMinDeployment" is pressure or altitude (true/false)
secMinDeploymentAlt: minimum altitude or pressure at which the second parachute can be opened
secDeploymentAlt: altitude at which the second parachute will fully deploy
secCutAlt: altitude at which the second parachute will automatically cut (set this to -1 if you don't want an autocut at a certain altitude)
secPreDeploymentSpeed: time required to predeploy the  second parachute in seconds (affects how fast it decelerates)
secDeploymentSpeed: time required to deploy the second parachute in seconds (affects how fast it decelerates)
secPreDeploymentAnimation: name of the predeployment animation of the second parachute (from the model)
secDeploymentAnimation: name of the deployment animation of the second parachute (from the model)
secParachuteName: name of the canopy of the second parachute (from the model)
secCapName: name of the protective cap of the second parachute (from the model)

--Material node--
MATERIAL
{
name: name of the material (string name you put in the PartModule)
areaDensity: density of the material in t/m²
dragCoefficient: drag coefficient of this material (recommended to not go below 0.5 and above 2)
}

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

//-------------------------------------------------------//
RealChute.dll was made by stupid_chris
The parts provided with the pack were all made by sumghai
The parachute cut sound was made by ymir9

Forum thread: http://forum.kerbalspaceprogram.com/threads/57988
Development thread: http://forum.kerbalspaceprogram.com/threads/57987

The plugin and parts are both licensed under CC-BY-NC-SA
http://creativecommons.org/licenses/by-nc-sa/3.0/
