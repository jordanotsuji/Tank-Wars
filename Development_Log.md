ChangeLog:

11/8 Initial setup. Created all the necessary world objects. Nothing functions at the moment.

11/10 Studied examples and lectures Started brainstorming and rough implementation of GameController, View to be implemented after GameController and Model are roughly functional ReceiveMessage implemented, ProcessMessages roughly implemented, waiting on Piazza answer

11/12 Decided to use one world implementation vs. two world buffering system Changed fields in the Model objects to Public Readonly Almost complete implementation ProcessMessages Rough implementation of IntialReceiveMessage

11/15 Changed fields in the Model to public with public get and private set Completed first implementation of all receieve and parsing operations (initial receive, regular receive, process messages, and parsing) Finalized namespaces for different portions of the program

11/16 Split the handshake methods into more methods Created a class to represent the commands to be sent to the server with toString method overriden to automatically Json Serialize Started working on the view, created DrawingPanel and some skeleton Used form editor and grids to format the buttons, text boxes, and labels for the form window Started to attempt drawing the gameworld, specifically, the background using DrawWithTransform and BackgroundDrawer

11/17 Reverted the change to our handshake methods, was causing the program to not send our playername before calling GetData Figured out that the DrawingPanel wasn't being properly integrated with our form, so we remade our form object and instead added all form elements manually through code. Figured out how to draw the background and started implementing the rest of the 'drawers' (TankDrawer, ProjectileDrawer, etc.) Put all provided sprites into a local folder under resources

11/18 Added math and graphics transformations so that the view is always centered around the player's tank Implemented most 'Drawers' into onPaint and successfully painting most world objects Some elements still a little slow in the view

11/19 Added a particle class to help with managing all of the particles that come along with an explosion Completed full implementation of all Drawers, successfully drawing all objects on each frame Started implementation of sending operations, successfully implemented left click, right click, and aiming

11/20 Implemented Rough movement, still choppy. Decided to change movement commands to a queueing system, resulting in super smooth movement Added a feature where the user can re-connect to another or the same server if the server disconnects the user through re-creation of controller and other essential parts of the program Added help button containing list of controlls

11/21 Final code cleanup, comments

11/22 Fixed reference issues, some settings like startup project weren't saved over GradeScope submission, so added extra instructions to the top of ReadMe file

------------------------------------------------Server Implementation ------------------------------------------------------

Instruction: Set the startup project to "Server" and then hit start (startup project changes don't save over github commit)

No extra gamemode.

11/28 First implementation of handshakes. Also implemented intersection methods in the Tank, Wall, and Beam classes. Started on a class to read and represent the game settings from an XML file. Next up is finish the config XML I/O, apply read messages, create game state sending loop, and finally implement game physics.

11/29 Implemented XML file reading into a GameSettings object. Created server sending loop, using a sleep to regulate frametime, and a command queue to ensure only the most recent command gets processed per frame. Rough tank aiming implementation.

11/30 Implemented more command processing. Tank can now move and will collide with walls. Tank can also spawn static projectiles. Implemented world wrapping in the tank class, and added a TankVelocity config parameter, default 3.0f. Decided to spawn projectiles at the tip of the turret rather than from the center of the tank.

12/01 Moved all movement and location logic from Tank to world Removed World reference in Tank constructor Completed projectile physics and logic Completed tank respawn procedure Added full list of configuration settings from PS9 instructions Ran into problem relating to clients disconnecting and the server waiting a full send loop before disposing of the Tank Added disposals for projectiles that have collided or left the map.

12/02 Now spawning powerups and able to shoot beams. Made server more efficient by only sending dead objects once. Added cleanup methods so dead projectiles and powerups aren't sent more than once Client disconnects seem to be unreliable for destroying the client's tank which is to be fixed later.

12/03 Fixed client disconnect issues where tanks would be destructed upon disconnection, but respawn later. Fixed by first, consolidating all of the methods called in the server that clear object dictionaries, into a cleanup() method and second, by moving the call to cleanup after the creation of the message to send to each client, instead of before. Consolidated the two methods used to process messages and then create a message to send into one method, in order to prevent having a gap between the two locks on the world that we previously had. Implemented the 'joined' variable that is set to true and sent on the first frame and first frame only a Tank joins.

12/04 Final comments Put the sending loop on its own thread. Fixed a bug where you were still alowed to shoot while dead Added a bool to check if its the tank's first shot, avoiding a problem where the tank couldn't shoot instantly upon joining or respawning
