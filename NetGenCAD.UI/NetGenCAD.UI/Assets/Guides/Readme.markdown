 # <img src="NetGenCAD.UI/NetGenCAD.UI/Assets/Images/logo.png" height="30" width="30"> NetGenCAD
###### 
#### Description
This application is a no-code tool for generating custom objects used in OpenSCAD, a solid 3D CAD modeler. This simplifies the process of creating complex 3D models by providing reusable components and utilities. **It is assumed that users have a basic understanding of OpenSCAD and its functionalities.**
######
If you're looking for only the axis tool, check out the NetGenCAD-Axis repository.
* **NetGenCAD Axis:** [Github](https://github.com/ogmarveler/NetSCAD-Axis)
######
#### Prerequisites
* **You need to have the following installed:** [OpenSCAD](https://openscad.org/downloads.html)
######
**Get the latest version of NetGenCAD**
######
* [NetGenCAD for Windows (x64)](NetGenCAD.UI/NetGenCAD.UI.Windows/NetGenCAD-0.2.0-winx64.7z)
* [NetGenCAD for Linux (x64 & arm64)](NetGenCAD.UI/NetGenCAD.UI.Linux/NetGenCAD-0.2.0-linux-x64-arm64.tar.gz)
![Designer Complex Object](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/coin.png)
![Designer Object Dark](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/designerObjectDark.png)

######
![Coin](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/redAlertTeslaCoil.png)
![Designer Object Dark](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/coilCreateDark.png)

######
#### Types of Solids Available
* **Cube** - default aligned on the 0,0,0 axes or can be offset with translate
* **Rounded Cube** - using Minkowski rounding with offsets to align with 0,0,0 axes
* **Cylinder** - aligned on the 0,0,0 axes, with the center point of the top of the cylinder (center circle)
* **Rounded Cylinder** - using Minkowski with offsets to align with 0,0,0 axes, center at top of cylinder
* **Surface** - import from png or dat file. Default aligned on the 0,0,0 axes or can be offset with translate.
* **Sphere** - all spheres are aligned on the 0,0,0 axes, with the center point of the sphere at 0,0,0.
###### 
#### Object Adjustments Available
* **Mirror** - mirror along X, Y, and/or Z axis when exporting or viewing the object
###### 
#### Layout of Custom Axes Builder
The Custom Axes Builder has 2 main visual sections: Custom Axis and Generated Axes. The outputs of newly created axes will show up in the tables next to the Custom Axis section.
###### 
![Axis Creation](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/axisCreation.png)
###### 
#### Types of Measurements
Varying axes of different sizes, measurement types, colors, and combinations of both metric and imperial measurements can be applied within the same SCAD project. Axes are managed in an aggregate SCAD file and are called as modules within your SCAD project. The app manages updates to existing axes as well as provides the ability for multiple axis types to be used in your project. Axis increments are as follows:
###### 
* **Metric axis** - 20mm, 10mm, 5mm, and 1mm increments
* **Imperial axis** - 1/4", 1/8", 1/16", and 1/32" increments
* **Convert mm to inches** - Enter inputs in Metric (mm) first, then select Imperial (in)
* **Convert inches to mm** - Enter inputs in Imperial (in) first, then select Metric (mm)

###### 
| Files                                   | Usage In Your SCAD File                                | Optional parameters | Render |
| --------------------------------------- |:------------------------------------------------------:|:-------------------:|:------:|
| Scad/Axes/custom_axis_name.scad         | use <Axes/axes.scad>; Get_Custom_Axis_Name();          | colorVal, alpha     |	No    |
| Scad/Solids/moduleType_name_object.scad | include <object_name_type.scad>;                       |                     |	Yes   |
| Scad/Solids/object.scad                 |                                                        |                     |	No    |	
| Scad/Solids/object.stl                  |                                                        |                     |  Yes   |