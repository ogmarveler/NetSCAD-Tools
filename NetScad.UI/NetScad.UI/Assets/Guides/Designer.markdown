 
# ![Logo](Assets/Images/logo.png) NetSCAD Designer Guide
###### 
#### Description
Welcome to the NetSCAD Designer Guide! This tutorial covers the basics of defining and generating custom objects used in OpenSCAD, a solid 3D CAD modeler. This project aims to simplify the process of creating complex 3D models by providing reusable components and utilities. Currently, it includes automating the creation of custom Imperial and Metric axes, as well as quick generation of objects to help speed up the modeling process. This allows for more precise modeling used in 3D printing.
###### 
#### Prerequisites
* **You need to have the following installed:** [OpenSCAD](https://openscad.org/downloads.html)
###### 
#### Types of Solids Available
Cubes and cylinders can be generated from the Designer module within this application. Rounded versions of cubes are also available in this iteration, in order to show how to offset Minkowski rounding to align with 0,0 axes. Future iterations will have more solid types available, but if needed, generated outputs can be modified within SCAD files directly. Solids available are as follows:
###### 
* **Cube** - default aligned on the 0,0,0 axes or can be offset with translate
* **Rounded Cube** - using Minkowski rounding with offsets to align with 0,0,0 axes
* **Cylinder** - all cylinders are aligned on the 0,0,0 axes, with the center point of the top of the cylinder (center circle)
###### 
#### Layout of Custom Object Designer
The Custom Object Designer is organized into several sections to facilitate the creation of custom objects for OpenSCAD. The main sections include:
###### 
![DesignerObject](Assets/Images/designerObject.png)
######
#### Apply a Custom Axis
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. Open the **Create** menu and select **Create New Object**                  |                                         |               |
| 2. Select an **Axis Type** to filter saved Imperial or Metric axes            |       Metric (mm) or Imperial (in)      |  Metric (mm)  |
| 3. Choose from **Select Axis** to apply one of the saved axes                 |     Dark or Light X x Y x Z mm or in    |  <required>   |
| 4. Optional: Enter numeric **Adjust X**, setting start point on **X Axis**    |     Units displayed in mm or inches     |       0       |
| 5. Optional: Enter numeric **Adjust Y**, setting start point on **Y Axis**    |     Units displayed in mm or inches     |       0       |
| 6. Optional: Enter numeric **Adjust Z**, setting start point on **Z Axis**    |     Units displayed in mm or inches     |       0       |

###### 
#### Creating A New Object
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 7. Enter an **Object Name**, used as the main identifier for solids, etc.     |           <Set Object Name>             |  <required>   |
| 8. Select a **Unit Type** to enter dimensions by Imperial or Metric units     |       Metric (mm) or Imperial (in)      |  Metric (mm)  |
| 9. Select a **Solid Type** to be added or subtracted from the object          |    Cube or Rounded Cube or Cylinder     |  <required>   |
| 10. Enter a name for the solid in the **Solid Description** textbox           |        <Set Object Description>         |  <required>   |
| 11. Select an **Apply To** type, which determines solid's relation to object  |    Union or Difference or Intersection  |     Union     |

###### 
#### Setting Inputs for New Solid: Cube and Rounded Cube
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 12. Optional: Select from presets to apply width, height, or cylinder radius  |     Rack Width or Rack Height           |               |
| 13. Enter numeric **Length** (based on 0 origin), which is on the **X Axis**  |     Units displayed in mm or inches     |  <required>   |
| 14. Enter numeric **Width** (based on 0 origin), which is on the **Y Axis**   |     Units displayed in mm or inches     |  <required>   |
| 15. Enter numeric **Height** (based on 0 origin), which is on the **Z Axis**  |     Units displayed in mm or inches     |  <required>   |
| 16. Optional: Enter numeric **Thickness** if solid is hollow or has walls     |     Units displayed in mm or inches     |               |
| 17. Optional: Enter numeric **X Offset**, setting start point on **X Axis**   |     Units displayed in mm or inches     |       0       |
| 18. Optional: Enter numeric **Y Offset**, setting start point on **Y Axis**   |     Units displayed in mm or inches     |       0       |
| 19. Optional: Enter numeric **Z Offset**, setting start point on **Z Axis**   |     Units displayed in mm or inches     |       0       |
| 20. Optional: Enter numeric **Rotate X**, rotating 0-360° on **X Axis**       |     Units displayed in degrees (°)      |       0°      |
| 21. Optional: Enter numeric **Rotate Y**, rotating 0-360° on **Y Axis**       |     Units displayed in degrees (°)      |       0°      |
| 22. Optional: Enter numeric **Rotate Z**, rotating 0-360° on **Z Axis**       |     Units displayed in degrees (°)      |       0°      |

###### 
#### Setting Inputs for New Solid: Cylinder
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 12. Optional: Select from presets to apply cylinder radius or cylinder height |     Screw Size or Screw Type            |               |
| 13. Enter numeric **Radius**, based on center to outer edge                   |     Units displayed in mm or inches     |  <required>   |
| 14. Optional: Enter numeric **Radius1**, based on center to outer edge        |     Units displayed in mm or inches     |               |
| 15. Optional: Enter numeric **Radius2**, based on center to inner edge        |     Units displayed in mm or inches     |               |
| 16. Enter numeric **Height** (based on 0 origin), which is on the **Z Axis**  |     Units displayed in mm or inches     |  <required>   |
| 17. Optional: Enter numeric **X Offset**, setting start point on **X Axis**   |     Units displayed in mm or inches     |       0       |
| 18. Optional: Enter numeric **Y Offset**, setting start point on **Y Axis**   |     Units displayed in mm or inches     |       0       |
| 19. Optional: Enter numeric **Z Offset**, setting start point on **Z Axis**   |     Units displayed in mm or inches     |       0       |
| 20. Optional: Enter numeric **Rotate X**, rotating 0-360° on **X Axis**       |     Units displayed in degrees (°)      |       0°      |
| 21. Optional: Enter numeric **Rotate Y**, rotating 0-360° on **Y Axis**       |     Units displayed in degrees (°)      |       0°      |
| 22. Optional: Enter numeric **Rotate Z**, rotating 0-360° on **Z Axis**       |     Units displayed in degrees (°)      |       0°      |

###### 
#### Object Action Buttons
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. **Create Solid** - save the inputs with select solid type to object        | Required settings inputs for solid      |               |
| 2. **Remove Solid** - remove a selected solid (row) from the object           | Selected row from object details        |               |
| 3. **Import Object** - get all details and items by existing object name      | Object Name                             |               |
| 4. **Clear Object** - clears all inputs, object tables, and object name       |                                         |               |
| 5. **Clear Inputs** - clear out any entered solid inputs                      |                                         |               |
| 6. **View Output** - preview the object in OpenSCAD                           |                                         |               |
| 7. **Remove/Apply Axis** - remove the axis before exporting (or reapply it)   |                                         |               |
| 8. **Modules** - update/add saved solids to object's specific modules         |                                         |               |

###### 
#### Viewing the Object in OpenSCAD
The object, along with any solids, is stored in a **Scad/Solids** folder that is included with the application. The main solid modules are in **object.scad**. **Rendering the axis is optional, based on your use case.** However, this can SIGNIFICANTLY INCREASE render time as it is primarily used for preview and development. Commenting out this part within the object.scad file will allow the object to render quickly. If building a ruler or some form of 1D or 2D measurement, then this is would be an example of why you would include the axis in rendering before final output.

###### 
| Files                                   | Usage In Your SCAD File                                | Optional parameters | Render |
| --------------------------------------- |:------------------------------------------------------:|:-------------------:|:------:|
| Scad/Axes/custom_axis_name.scad         | use <Axes/axes.scad>; Get_Custom_Axis_Name();          | colorVal, alpha     |	No    |
| Scad/Solids/moduleType_name_object.scad | include <object_name_type.scad>;                       |                     |	No    |
| Scad/Solids/object.scad                 |                                                        |                     |	Yes   |	
| Scad/Solids/object.stl                  |                                                        |                     |  Yes   |