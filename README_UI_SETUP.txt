SPIROGRAPH UI SETUP INSTRUCTIONS
=================================

HOW TO GENERATE THE UI:
1. Create an empty GameObject in your scene (GameObject > Create Empty)
2. Name it "UIManager"
3. Add the SpirographUIManager component to it (Add Component > SpirographUIManager)
4. In the Inspector, check the "Generate UI" checkbox
5. The UI will be created automatically! You'll see:
   - SpirographCanvas in the Hierarchy
   - ControlPanel with all sliders and buttons
   - EventSystem for input handling

WHAT GETS CREATED:
- Speed Slider (0.1 to 200)
- Cycles Slider (1 to 500)  
- Rotation Speed Slider (-10 to 10)
- Pause/Resume Button
- Reset Button
- Free Fly Button (camera mode - QZSD movement)
- Follow Target Button (camera mode - orbital)
- Mat 1, Mat 2, Mat 3, Mat 4 Buttons (trail color switching)
- Camera instructions text

CUSTOMIZING THE UI:
After generation, you can freely edit:
- Colors: Select any button/slider > Inspector > Image component > Color
- Sizes: Select any element > Rect Transform > Width/Height
- Positions: Select any element > Rect Transform > Pos X/Y
- Fonts: Change the text components
- Add background images or icons

THE UI WILL AUTOMATICALLY CONNECT TO:
- SpirographRoller script (speed, cycles, pause, reset, material buttons)
- RotateParent script (rotation speed slider)
- CameraController script (Free Fly/Follow Target buttons)

These scripts will find the UI elements by name and connect to them in their Start() methods.

SETTING UP MATERIAL BUTTONS:
1. Create 4 materials in Unity with different colors (right-click > Create > Material)
2. Assign each material a unique color or texture
3. Select the GameObject with SpirographRoller component
4. In the Inspector, find "Trail Materials" section
5. Drag your 4 materials into Material1, Material2, Material3, Material4 slots
6. The buttons will now switch between these materials in real-time!

IMPORTANT NOTES:
- Only generate the UI once! Checking "Generate UI" again will create duplicates
- If you want to regenerate, delete the old Canvas first
- The UI Manager GameObject can be deleted after generation if desired
- All slider values and button clicks will automatically work with the scripts

INPUT SYSTEM COMPATIBILITY:
- Works with both Legacy Input and New Input System
- EventSystem will automatically use the correct input module
- Camera uses legacy Input (works in all Unity configurations)

TROUBLESHOOTING:
- If UI doesn't appear: Check Canvas sorting order is 100
- If buttons don't respond: Check EventSystem exists in scene
- If sliders don't update: Make sure script components are enabled
- Console errors: Check that all three scripts are in the project

Enjoy your spirograph!
