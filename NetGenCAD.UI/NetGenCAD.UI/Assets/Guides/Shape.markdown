 
# ![Logo](Assets/Images/logo.png) NetGenCAD Shape Creation Guide
###### 
#### Description
Welcome to the NetGenCAD Shape Creation Guide! This tutorial covers the basics of defining and generating custom shapes used in OpenSCAD, a solid 3D CAD modeler.
###### 
#### Layout of Custom Shape Designer
There are 5 main sections: applying a custom axis, creating a new shape or retrieving an existing one, setting inputs for a new polyhedron, the list of points in the shape, and faces that contain the points. Once a face is added to the shape, it will appear in the Faces table, along with its parameters. To view the shape in OpenSCAD, simply click the **Preview Shape** button, which will open the corresponding SCAD file.

###### 
![DesignerObject](Assets/Images/polyhedronInputsDark.png)

#### Apply a Custom Axis
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. Open the **Create** menu and select **Create New Shape**                   |                                         |               |
| 2. Select an **Axis Type** to filter saved Imperial or Metric axes            |       Metric (mm) or Imperial (in)      |  Metric (mm)  |
| 3. Choose from **Select Axis** to apply one of the saved axes                 |     Dark or Light X x Y x Z mm or in    |  <required>   |
| 4. Optional: Enter numeric **Adjust X**, moving start point on **X Axis**     |     Units displayed in mm or inches     |       0       |
| 5. Optional: Enter numeric **Adjust Y**, moving start point on **Y Axis**     |     Units displayed in mm or inches     |       0       | 
| 6. Optional: Enter numeric **Adjust Z**, moving start point on **Z Axis**     |     Units displayed in mm or inches     |       0       |

###### 
#### Creating A New Shape
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 7. Enter an **Shape Name**, used as main identifier for points, faces, etc.   |           <Set Shape Name>              |  <required>   |
| 8. Select a **Unit Type** to enter dimensions by Imperial or Metric units     |       Metric (mm) or Imperial (in)      |  Metric (mm)  |
| 9. Enter a name for the point or face in the **Description** textbox          |        <Set Point or Face Description>  |  <required>   |
| 10. Select an **Apply To** type, either creating a point or face              |    Points or Faces                      |     Points    |

###### 
#### Setting Inputs for New Point
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 11. Enter numeric **X Coordinate**, defining a point's position on **X Axis** |     Units displayed in mm or inches     |       0       |
| 12. Enter numeric **Y Coordinate**, defining a point's position on **Y Axis** |     Units displayed in mm or inches     |       0       |
| 13. Enter numeric **Z Coordinate**, defining a point's position on **Z Axis** |     Units displayed in mm or inches     |       0       |
| 14. Enter numeric **Point ID**, autoincrement or manually set face point      |       0 and above (integer only)        |       0       |

###### 
#### Setting Inputs for New Face
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 11. Enter numeric **Face Points**: comma-separated, defining face's vertices  | OpenSCAD syntax require open/close [..] |  <required>   |
| 12. Enter numeric **Face ID**, autoincrement or manually set face ID          |       0 and above (integer only)        |       0       |
	
###### 
#### Shape Action Buttons
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. **Create Point or Face** - create or updates a point or face in the shape  | Required inputs for point or face       |               |
| 2. **Import Shape** - get all details and items by existing shape name        | Shape Name                              |               |
| 3. **Clear Shape** - clears all inputs, polyhedron tables, and shape name     |                                         |               |
| 4. **Clear Inputs** - clear out any entered point or face inputs              |                                         |               |
| 5. **Preview Shape** - preview the shape in OpenSCAD                          | <Shape must contain at least one face>  |               |
| 6. **Save Shape** - save the current shape for use in an object               |                                         |               |
| 7. **View Code** - view the OpenSCAD code generated from points and faces     |                                         |               |
| 8. **All Solids** - apply changes to all objects using the shape as a solid   | <Shape must be saved first>             |               |

###### 
#### Points and Faces Tables Actions
| Actions                                                                       | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. **Trash Bin Icon** - remove selected row from the points or faces table    |       Points or Faces Table             |               |
| 2. **Sorting** - click on one or more column headers to sort (shift + click)  |       Points or Faces Table             |               |

###### 
#### Saved Shape Table (Read-Only)
Once a shape is saved, it will appear in the Saved Shapes table. This table is read-only and cannot be edited directly. To make changes to a saved shape, you must first import it into the designer using the **Import Shape** button. After making the desired changes, you can save the updated shape with the same name.

###### 
#### Generated SCAD Files
| Files                                   | Usage In Your SCAD File / Description                  | Optional parameters | Render |
| --------------------------------------- |:------------------------------------------------------:|:-------------------:|:------:|
| Scad/Axes/custom_axis_name.scad         | Syntax: use <Axes/axes.scad>; Get_Custom_Axis_Name();  | colorVal, alpha     |	No    |
| Scad/Solids/polyhedronName_shape.scad   | Description: polyhedron shape preview file             |                     |	No    |