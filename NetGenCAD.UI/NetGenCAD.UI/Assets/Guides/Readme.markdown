# ![Logo](Assets/Images/logo.png) NetGenCAD
###### 
#### Description
This application is a no-code tool for generating custom objects used in OpenSCAD, a solid 3D CAD modeler. **It is assumed that users have a basic understanding of OpenSCAD and its functionalities.** 
###### 
#### Prerequisites
* **You need a CAD renderer installed (OpenSCAD recommended):** [OpenSCAD](https://openscad.org/downloads.html)
* [NetGenCAD for Windows (x64)](NetGenCAD.UI/NetGenCAD.UI.Windows/NetGenCAD-0.1.0-win-x64.7z)
* **NetGenCAD for Linux (x64 & arm64)** Coming Soon!
* **NetGenCAD for MacOS (x64 & arm64)** Coming Soon!

###### 
#### Types of Solids Available
* **Cube** - default aligned on 0,0,0 axes. Can be offset with Adjust X, Y, Z values.
* **Cylinder** - all cylinders are aligned on 0,0,0 axes, with center point at center of circle.
* **Polyhedron** - created in Shape Designer, and multiple polyhedrons can be used within Object Designer.
* **Rounded Cube** - Minkowski rounding using offsets to align to 0,0,0 axes
* **Rounded Cylinder** - Minkowski rounding using offsets to align to 0,0,0 axes. Center is center of circle.
* **Sphere** - all spheres are aligned on the 0,0,0 axes, with center point of the sphere at 0,0,0.
* **Surface** - import from png or dat file. Default aligned on 0,0,0 axes or can be offset with translate.
* **Text** - create 3D text using OpenSCAD text function with approximate bounding box dimensions.

###### 
#### Examples of Objects Created
###### 
![Bullion](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/bullionBars.png)
![Bullion Create Dark](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/bullionBarsCreateDark.png)

###### 
![Coin](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/coin.png)
![Coin Create Dark](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/coinCreateDark.png)

###### 
![Coil Create Dark](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/dataPyramid.png)
![Polyhedron Inputs Dark](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/polyhedronInputsDark.png)

###### 
![Coil](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/redAlertTeslaCoil.png)
![Coil Create Dark](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/coilCreateDark.png)

###### 
#### Solid Adjustments Available
* **Rotate** - rotate along X, Y, and/or Z axis when creating/updating a solid
* **Scale** - scale along X, Y, and/or Z axis when importing an image or dat file
* **Translate** - place the solid at specified position, along X, Y, and/or Z axis
* **Color** - apply color to the solid being created/updated
* **Alpha** - apply transparency to the solid being created/updated
* **Layer** - exclusive NetGenCAD feature for specifying solid difference or intersections

###### 
#### Object Adjustments Available
* **Mirror** - mirror along X, Y, and/or Z axis when exporting or viewing the object
* **Copy** - create duplicates of the object being created/updated
* **Render** - applies optimized rendering of solids before viewing or exporting the object
* **Export** - export the object as an STL file for 3D printing or other uses

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
#### Generated SCAD Files
| Files                                   | Usage In Your SCAD File / Description                  | Optional parameters | Render |
| --------------------------------------- |:------------------------------------------------------:|:-------------------:|:------:|
| Scad/Axes/axes.scad                     | Description: holds all stored axes used in 3D models   |                     |	No    |
| Scad/Axes/custom_axis_name.scad         | Syntax: use <Axes/axes.scad>; Get_Custom_Axis_Name();  | colorVal, alpha     |	No    |
| Scad/Solids/moduleType_name_object.scad | Syntax: include <object_name_type.scad>;               |                     |	Yes   |
| Scad/Solids/object.scad                 | Description: main object file for 3D model             |                     |	No    |	
| Scad/Solids/object.stl                  | Description: exported STL file for 3D model            |                     |  Yes   |
| Scad/Solids/polyhedronName_shape.scad   | Description: polyhedron shape preview file             |                     |	No    | 
