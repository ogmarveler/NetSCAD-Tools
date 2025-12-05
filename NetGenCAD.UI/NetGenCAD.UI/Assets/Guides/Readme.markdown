 # <img src="NetGenCAD.UI/NetGenCAD.UI/Assets/Images/logo.png" height="30" width="30"> NetGenCAD
###### 
#### Description
This application is a no-code tool for generating custom objects used in OpenSCAD, a solid 3D CAD modeler. This simplifies the process of creating complex 3D models by providing reusable components and utilities. **It is assumed that users have a basic understanding of OpenSCAD and its functionalities.**
######
#### Latest Updates to NetGenCAD
######
**Version 0.2.0** 
* New solid added: Sphere.
* Added ability to create layers within custom objects. Layers allow for grouping of solids within the object, making it easier to manage complex designs. 
* Alpha channel support has been added for objects. 
* Improved UI/UX for better user experience.
* ![Designer Complex Object](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/coilofrings.png)
###### 
#### Other NetGenCAD Projects
If you're looking for only the axis tool, check out the NetGenCAD-Axis repository.
* **NetGenCAD Axis:** [Github](https://github.com/ogmarveler/NetSCAD-Axis)
######
#### Prerequisites
* **You need to have the following installed:** [OpenSCAD](https://openscad.org/downloads.html)
######
![Coin](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/coin.png)
![Designer Object Dark](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/designerObjectDark.png)
######
#### Layout of Custom Object Designer
There are 5 main sections: applying a custom axis, creating a new object or retrieving an existing one, setting inputs for new solids, the list of Solids in the object, and Modules that contain the Solids. Once a solid is added to the object, it will appear in the Solids table, along with its parameters. Solids can be viewed within Modules, which define how they interact with each other. To view the object in OpenSCAD, simply click the **View Output** button, which will open the corresponding SCAD file. To export the object as an STL file, click the **Export** button. When making changes, be sure to click the **Update Modules** button to refresh the object.
######
![Mini PC](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/miniPC.png)
![Designer Object Dark Cube](NetGenCAD.UI/NetGenCAD.UI/Assets/Images/designerObjectDarkCube.png)
######
#### Rendering/Previewing the Object in OpenSCAD
The object, along with any solids, is stored in a **Scad/Solids** folder that is included with the application. The main solid modules are in **object.scad**. Click the **Remove Axis** within the object.scad file to comment out the axis. If exporting to STL, this will be done automatically. If building a ruler or some form of 1D or 2D measurement, then this is would be an example of why you would include the axis in rendering before final output.

######
#### Types of Solids Available
* **Cube** - default aligned on the 0,0,0 axes or can be offset with translate
* **Rounded Cube** - using Minkowski rounding with offsets to align with 0,0,0 axes
* **Cylinder** - all cylinders are aligned on the 0,0,0 axes, with the center point of the top of the cylinder (center circle)
* **Surface** - import from png or dat file. Default aligned on the 0,0,0 axes or can be offset with translate.
* **Sphere** - all spheres are aligned on the 0,0,0 axes, with the center point of the sphere at 0,0,0.
###### 
#### Object Adjustments Available
* **Mirror** - mirror along X, Y, and/or Z axis when exporting or viewing the object
###### 
#### Layout of Custom Axes Builder
The Custom Axes Builder has 2 main visual sections: Custom Axis and Generated Axes. The outputs of newly created axes will show up in the tables next to the Custom Axis section. This shows the total cubic size, as well as the name to use in your project file. In the list below the custom axis settings, you will see the newly created axis, along with axes that have been previously created. There are two tables that display axes information. One is metric axes and the other is imperial axes. **Both ascending and descending sorting are possible, as well as sorting by multiple categories (shift + select).**
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