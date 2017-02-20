/*
 * gs.java
 * started: 4/1/2011
 *
 * author: Skyler Goodell 'GoodSky'
 * 
 * Dedicated to anyone who wants to learn the art of programming.
 *
 * p.s. oink oink boom!
 */
package goodsky;

// Program Dependencies
import java.awt.*;
import java.awt.event.*;
import java.awt.image.*;

import javax.swing.*;
import javax.imageio.*;

import java.io.*;

import java.util.*;

/**
 * This is the main class for the GoodSky Game Library. This class is not meant
 * to be very object oriented, in fact this class contains primarily static
 * methods for the user to use. This helps learning Java students so they can
 * get started making games sooner, and hopefully keep them entertained with
 * programming to improve their skills.
 * 
 * @author Skyler Goodell
 */
public class gs extends JFrame {

    // Static Fields
    protected static gs base = null;
    private static boolean running; // note: this should always be true... kinda hax

    // Fields
    // Stage Height and Width
    private int height = 500;
    private int width = 800;
    private Color bColor = Color.BLACK;

    // Game FPS (in delay ms per iteration)
    private long fps_delay;
    private long lastiteration;
    private double fps_actual;

    // Mouse fields
    private int mX;
    private int mY;
    private boolean mLK;
    private boolean mRK;

    // Key fields
    private boolean[] letterkeys = new boolean[26];
    private boolean[] arrowkey = new boolean[4];
    private boolean   spacekey = false;
    private boolean   shiftkey = false;
    private boolean   enterkey = false;

    // Stage Canvas where all the game happens
    private Canvas canvas;

    // Buffer to double buffer our drawing
    private Image iBuf;
    private Graphics gBuf;

    // Background where we store the... well background (only if the user loads one)
    private BufferedImage background = null;
    private BufferedImage obackground = null;
    private int camerax = 0;
    private int cameray = 0;

    /** The collection of game objects to render */
    protected ArrayList<GameObject> objects = new ArrayList<GameObject>();
    protected ArrayList<Text> texts = new ArrayList<Text>();

    // Static Constants for different things
    /** Code for the LEFT Arrow Key */
    public static final int LEFT_ARROW = 0;
    /** Code for the UP Arrow Key */
    public static final int UP_ARROW = 1;
    /** Code for the RIGHT Arrow Key */
    public static final int RIGHT_ARROW = 2;
    /** Code for the DOWN Arrow Key */
    public static final int DOWN_ARROW = 3;

    /** Code for a CIRCLE Shape in a Game Object */
    public static final int CIRCLE = 0;
    /** Code for a SQUARE Shape in a Game Object */
    public static final int SQUARE = 1;
    /** Code for a TRIANGLE Shape in a Game Object */
    public static final int TRIANGLE = 2;

    /** Code for Bounding Box collision with collision methods */
    public static final int COL_BOX = 0;
    /** Code for Circular Bounds collision with collision methods */
    public static final int COL_CIRCLE = 1;
    /** Code for Pixel Perfect collision with collision methods */
    public static final int COL_PIXEL = 2;
    
    /**
     *  A static function call that will populate the base field.
     *  It is required to make this function call before you
     *  do anything else with the library.
     */
    public static void start()
    {
        if (base == null)
        {
            // Create the game window
            base = new gs();

            running = true;
        }
        else
            throw new GoodSkyException("You have already started the GoodSky Game Engine");
    }

    /**
     * Use this to make your main game loop
     *
     * @return this will return true when the program is still running and false
     * once the program is complete
     */
    public static boolean gameloop()
    {
        return running;
    }

    /**
     * This will render all our game objects to the screen and wait a certain amount
     * to keep stable FPS.
     *
     * NOTE: You MUST call this function in your main game loop for ANYTHING to happen.
     */
    public static void sync()
    {
        // Choose the graphics for the sync
        Graphics2D g = (Graphics2D)base.gBuf;

        // Clear the background with a solid color or a background color if supplied
        if (base.background != null)
        {
            g.drawImage(base.background, 0, 0, base);
        }
        else if (base.bColor != null)
        {
            g.setColor(base.bColor);
            g.fillRect(0, 0, base.width, base.height);
        }

        // Draw all objects in the objects ArrayList
        for (GameObject obj : base.objects)
        {
            if (!obj.visible) continue;

            // I will change this to modify with the camera position
            // And only draw the image if they are in the camera bounding box
            obj.updateBound();
            if ((int)obj.x - (obj.boundwidth/2) < base.width + base.camerax && (int)obj.x + (obj.boundwidth/2) > base.camerax &&
                    (int)obj.y - (obj.boundheight/2) < base.height + base.cameray && (int)obj.y + (obj.boundheight/2) > base.cameray)
                g.drawImage(obj.getImage(), (int)obj.x-(obj.boundwidth/2) - base.camerax, (int)obj.y-(obj.boundheight/2) - base.cameray, base);
        }

        // Draw all the text
        for (int i = base.texts.size()-1; i >= 0; i--)
        {
            g.setColor(base.texts.get(i).col);
            g.drawString(base.texts.get(i).msg, base.texts.get(i).x, base.texts.get(i).y);
            base.texts.remove(i);
        }

        // Flip the buffer
        base.canvas.getGraphics().drawImage(base.iBuf, 0, 0, base);

        // Wait to keep the FPS somewhat constant
        long now = System.currentTimeMillis();
        base.fps_sync(base.fps_delay - (now - base.lastiteration));

        // Calculate the FPS for testing use
        base.fps_actual = (int)Math.ceil((double)1000/(now - base.lastiteration));
        if (base.fps_actual > 1000/base.fps_delay) base.fps_actual = 1000/base.fps_delay;

        base.lastiteration = now;
    }

    /**
     * Wait for a certain period of time on the thread. This happens to stabilize FPS.
     * @param time: time in ms to wait
     */
    private void fps_sync(long time)
    {
        if (time > 0)
        {
            try {
                Thread.sleep(time);
            }
            catch (Exception e) { }
        }
    }

    /**
     * Sets the desired frames per second
     */
    public static void setFPS(int fps)
    {
        base.fps_delay = 1000/fps;
    }

    /**
     * This function tells you what the current Frames per Second is of the project.
     * @return an integer estimating the frames per second
     */
    public static double getFPS()
    {
        return base.fps_actual;
    }

    /**
     * add an object to the render list
     * @param obj object to be rendered
     */
    protected static void addObject(GameObject obj)
    {
        base.objects.add(obj);
        Collections.sort(base.objects);
    }

    /**
     * remove an object from the render list
     * @param obj object to be removed
     */
    protected static void removeObject(GameObject obj)
    {
        base.objects.remove(obj);
    }

    /**
     * This will change the size of the screen that we run the program on.
     *
     * @param width
     * @param height
     */
    public static void setScreenSize(int width, int height)
    {
        base.width = width;
        base.height = height;
        
        //base.setSize(width, height);
        base.canvas.setSize(width, height);
        base.pack();

        base.gBuf.dispose();

        base.iBuf = new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB);
        base.gBuf = base.iBuf.getGraphics();
    }

    public static void setScreenTitle(String title)
    {
        base.setTitle(title);
    }

    /**
     * Set the background color of the screen.
     * @param c the color you wish to change the background to. e.g. Color.BLUE
     */
    public static void setBackgroundColor(Color c)
    {
        base.bColor = c;
    }

    /**
     * Only use this command if you really know what you are doing.
     */
    public static void setBackgroundOff()
    {
        base.background = null;
        base.bColor = null;
    }

    /**
     * Set the background image if you choose to use an image for the background.
     * NOTE: if you want to remove a background you can always set the image to null.
     * @param filename The path to the file you want to make the background. e.g. "background.bmp"
     */
    public static void setBackgroundImage(String filename)
    {
        try {
            base.obackground = ImageIO.read(new File(filename));

            // check for cutting off on the edge
            if (base.obackground.getWidth() < base.width || base.obackground.getHeight() < base.height)
                throw new GoodSkyException("Background images must be larger or equal to the size of the stage.");
            else
                base.background = base.obackground.getSubimage(base.camerax, base.cameray, base.width, base.height);

        } catch (IOException e) {
            throw new GoodSkyException("BUMMER! I couldn't load the image file for the background! Make sure you typed the file name correctly!");
        }
    }

    /**
     * Position the virtual camera at position x and y. The camera can not go below 0,0
     * or beyond backgroundimage.width - stage.width, backgroundimage.height - stage.height.
     *
     * @param x Position of the camera in x (top left corner)
     * @param y Position of the camera in y (top left corner)
     */
    public static void setCameraPosition(int x, int y)
    {
        base.camerax = x;
        base.cameray = y;

        if (base.camerax < 0) base.camerax = 0;
        if (base.cameray < 0) base.cameray = 0;
        if (base.camerax > base.obackground.getWidth() - base.width) base.camerax = base.obackground.getWidth() - base.width;
        if (base.cameray > base.obackground.getHeight() - base.height) base.cameray = base.obackground.getHeight() - base.height;
        base.background = base.obackground.getSubimage(base.camerax, base.cameray, base.width, base.height);
    }

    /**
     * This will put up a title screen using the image supplied. This is only the most simple title screen, if you wish for a title menu with more advanced functionality then you will have to
     * write your own. Once you click on the screen the function will exit and continue on to your game.
     * @param imagefile Make this your title image. It should be the same size as the screen, otherwise you may get some strange results.
     */
    public static void TitleScreen(String imagefile)
    {
        // Grab the screen Graphics
        Graphics2D g = (Graphics2D)base.gBuf;

        Image titleOverlay;

        try {
            titleOverlay = ImageIO.read(new File(imagefile));
        }
        catch (Exception e) {
            throw new GoodSkyException(("Could not open the image you supplied for the title screen : ( Try again!"));
        }
        // Show the overlay while you wait for a key press
        while (!mouseLeft())
        {
            base.canvas.getGraphics().drawImage(titleOverlay, 0, 0, base);
            base.fps_sync(100);
        }
    }

    /**
     * Stop the program and exit
     */
    public static void exit()
    {
        running = false;
        base.setVisible(false);
        System.exit(0);
    }

    /**
     * Returns the x position of the camera.
     * @return x position of the camera.
     */
    public static int getCameraX()
    {
        return base.camerax;
    }

    /**
     * Returns the y position of the camera.
     * @return y position of the camera.
     */
    public static int getCameraY()
    {
        return base.cameray;
    }

    /**
     * Get the x position of the mouse on the screen
     * @return x position of the mouse
     */
    public static int mouseX()
    {
        return base.mX;
    }

    /**
     * Get the y position of the mouse on the screen
     * @return y position of the mouse
     */
    public static int mouseY()
    {
        return base.mY;
    }

    /**
     * See if the left mouse button is pressed.
     *
     * @return true if the left mouse button is down and false otherwise.
     */
    public static boolean mouseLeft()
    {
        return base.mLK;
    }

    /**
     * See if the right mouse button is pressed.
     *
     * @return true if the right mouse button is down and false otherwise.
     */
    public static boolean mouseRight()
    {
        return base.mRK;
    }

    /**
     * Returns if one of the arrow keys is down.
     * @param keycode The key to check e.g. GoodSky.LEFT_ARROW
     * @return true if the key is down. false if the key is not down.
     */
    public static boolean getArrowKey(int keycode)
    {
        return base.arrowkey[keycode];
    }

    /**
     * Returns if one of the character keys is down.
     * @param letter The character you want to see if it is down. e.g. 'W'
     * @return true if the key is down. false if the key is not down.
     */
    public static boolean getKey(char letter)
    {
        int keycode = (int)letter;
        if (keycode > 90) keycode -= 32;
        if (keycode >= 65 && keycode <= 90)
            return base.letterkeys[keycode - 65];
        return false;
    }

    /**
     * Returns if the space key is down.
     * @return true if the key is down. false if the key is not down.
     */
    public static boolean getSpaceKey()
    {
        return base.spacekey;
    }

    /**
     * Returns if the shift key is down.
     * @return true if the key is down. false if the key is not down.
     */
    public static boolean getShiftKey()
    {
        return base.shiftkey;
    }

    /**
     * Returns if the enter/return key is down.
     * @return true if the key is down. false if the key is not down.
     */
    public static boolean getEnterKey()
    {
        return base.enterkey;
    }

    /**
     * Print text to screen. Useful for debugging tests.
     *
     * @param msg What you want to print to the screen
     * @param x the x position to print to the screen
     * @param y the y position to print to the screen
     */
    public static void text(String msg, int x, int y)
    {
        base.texts.add(new Text(x, y, msg, Color.WHITE));
    }

    public static void text(String msg, int x, int y, Color c)
    {
        base.texts.add(new Text(x, y, msg, c));
    }

    /**
     * Constructor: Sets up the swing window and canvas. User does not have to call this.
     */
    public gs()
    {
            // Set up the game window parameters
            setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
            setResizable(false);

            // Set up the canvas parameters
            canvas = new Canvas();
            canvas.setSize(width, height);
            canvas.setBackground(Color.BLACK);

            // Add canvas to our JFrame
            add(canvas);
            pack();

            // Set up the buffer as well
            iBuf = new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB);
            gBuf = iBuf.getGraphics();

            // Start the event listeners
            canvas.addMouseListener(new GoodSkyMouseListener());
            canvas.addMouseMotionListener(new GoodSkyMouseMoveListener());
            canvas.addKeyListener(new GoodSkyKeyListener());

            // Set up some game parameters
            fps_delay = 1000/40; //defaults to 40fps
            fps_actual = 0;
            lastiteration = System.currentTimeMillis();

            // Center and display the JFrame
            setLocationRelativeTo(null);
            setVisible(true);
    }

    // *********************************************************
    //  ABSTRACT METHODS
    //  -implementing them through inner classes
    // *********************************************************
    /**
     * Handles the mouse clicking methods.
     */
    private class GoodSkyMouseListener implements MouseListener
    {

        public void mouseClicked(MouseEvent e) {
        }

        public void mousePressed(MouseEvent e) {
            if (e.getButton() == MouseEvent.BUTTON1)
                mLK = true;
            else if (e.getButton() == MouseEvent.BUTTON3)
                mRK = true;
        }

        public void mouseReleased(MouseEvent e) {
            if (e.getButton() == MouseEvent.BUTTON1)
                mLK = false;
            else if (e.getButton() == MouseEvent.BUTTON3)
                mRK = false;
        }

        public void mouseEntered(MouseEvent e) {
        }

        public void mouseExited(MouseEvent e) {
        }
    }

    /*
     * Handles the mouse movement methods.
     */
    private class GoodSkyMouseMoveListener implements MouseMotionListener
    {
        public void mouseDragged(MouseEvent e) {
            mX = e.getX();
            mY = e.getY();
        }

        public void mouseMoved(MouseEvent e) {
            mX = e.getX();
            mY = e.getY();
        }
    }

    /**
     * Handles the keyboard listening methods
     */
    private class GoodSkyKeyListener implements KeyListener
    {
        public void keyTyped(KeyEvent e) {
        }

        public void keyPressed(KeyEvent e) {
            int key = e.getKeyCode();
            // alphabetic keys
            if (key >= 65 && key <= 90)
                letterkeys[key-65] = true;
            // arrow keys
            else if (key >= 37 && key <= 40)
                arrowkey[key-37] = true;
            // space key
            else if (key == 32)
                spacekey = true;
            // shift key
            else if (key == 16)
                shiftkey = true;
            // enter key
            else if (key == 10)
                enterkey = true;
        }

        public void keyReleased(KeyEvent e) {
            int key = e.getKeyCode();
            // alphabetic keys
            if (key >= 65 && key <= 90)
                letterkeys[key-65] = false;
            // arrow keys
            else if (key >= 37 && key <= 40)
                arrowkey[key-37] = false;
            // space key
            else if (key == 32)
                spacekey = false;
            // shift key
            else if (key == 16)
                shiftkey = false;
            // enter key
            else if (key == 10)
                enterkey = false;
        }
    }

    // *********************************************************
    // A nested class to hold information about text!!!
    // *********************************************************
    private static class Text
    {
        protected int x;
        protected int y;
        protected String msg;
        protected Color col;

        public Text(int x, int y, String msg, Color col)
        {
            this.x = x;
            this.y = y;

            this.msg = msg;
            this.col = col;
        }
    }
}
