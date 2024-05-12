# tyre simulation
 simulation of tyre motion for tbd car game

## Unity 6 project
 a build in folder `builds`  
  - left click mouse to open values tweak menu  
   
 requires Unity AssetStore assets to be put in folder `Unity project/Assets/Third-Party`:  
  - Easy Tweak (https://assetstore.unity.com/packages/tools/gui/easy-tweak-162205)  
  - Quick Outline (https://assetstore.unity.com/packages/tools/particles-effects/quick-outline-115488)  
   
 YouTube demo: https://youtu.be/foVWJFGZllQ

## current state
 **tyre physics implemented**:
 - linear momentum
 - spring effect (dampened spring + visual deformation)
 - rotation due to linear momentum on contact
 - friction (air, roll, transversal slip)

 **not implemented (yet)**:
 - roll due to gravity (downhill roll behaviour observed is due to spring effect, gravity, rotation due to linear momentum on contact and friction)
 - angular momentum from inertia due to transversal tilt on contact
 - tyre to tyre contact

 **tyre properties**:
 - radius 0.991199m
 - width 1m
 - weight 40kg

 - pressure 0.25
 - dampening 1200
 - spring constant 30000

 **spring code excerpt**:
```C#
// tyre acts like dampened spring
// dampen force is proportionally tyre movement in spring force dir as long as tyre surface contacts the ground
// spring force dir is tyreSquishForceDir
// when tyre surface looses contact to ground, tyre surface stops moving as spring instantly (this is an approximation here)
// springTyreAcceleration is acceleration of the whole tyre due to the acting like a spring
min = radius to tyre surface indentation
tyreSquishForceVal = (tyreRadius - min) / unitTyrePressure;
springTyreAcceleration = (tyreSpringConstant * tyreSquishForceVal - dampening * Vector3.Dot(velocity, indentationDir)) * indentationDir / tyreMass;


static Vector3 gravity = new Vector3(0, -9.81f, 0);
const float airFrictionCoefficient = 0.01f;
const float rollFrictionEarthRubberCoefficient = 0.0013f;
const float slideFrictionEarthRubberCoefficient = 100.0f;
```
  
