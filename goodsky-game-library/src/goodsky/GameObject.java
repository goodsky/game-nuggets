/*
 * This class holds the necessary graphics and information for one single
 * game object.
 * 
 */
package goodsky;

import java.awt.*;
import java.io.*;
import java.util.*;
import java.awt.image.*;
import java.awt.geom.AffineTransform;
import java.util.ArrayList;
import javax.imageio.*;

/**
 * The GameObject holds all information about objects that you want to move
 * around on the screen.
 * 
 * @author Skyler Goodell
 */
public class GameObject implements Comparable<GameObject> {


    // The original image is stored in an array list 
    // just in case we have animations, then we can store them all here
    // otherwise we have an arraylist of size 1
    /** Internal Field: Do not worry about this */
    protected ArrayList<BufferedImage> originalimage = new ArrayList<BufferedImage>();

    // image holds the last render of this object, this is useful for pixel collision
    // and also so we only have to re-transform the image when necessary
    /** Internal Field: Do not worry about this */
    protected BufferedImage image = null;

    // Original width and height of the buffered image
    private int owidth;
    private int oheight;

    // Frames and Animation
    private int frame = 0;
    private int oldframe = 0;
    private int framedelay = 20;
    private int framecounter = 0;
    private boolean animated = false;

    /** The x Position of the object on the screen */
    public double x = 0;
    /** The y Position of the object on the screen */
    public double y = 0;
    /** Internal Field: Do not worry about this */
    protected int depth = 10;
    /** Internal Field: Do not worry about this */
    protected boolean visible = true;

    /** Internal Field: Do not worry about this */
    protected int boundwidth;
    /** Internal Field: Do not worry about this */
    protected int boundheight;

    // These fields hold the transformation needed
    private double xScale = 1.0;
    private double yScale = 1.0;
    private double angle = 0.0;

    // Hold these variables so we only do the math on transformations when needed
    private double oldxScale = xScale;
    private double oldyScale = yScale;
    private double oldangle = angle;

    // These are for manual bounding
    // Negative numbers means we will default to the auto-generated bounding values
    private int boxcolwidth = -1;
    private int boxcolheight = -1;
    private int circolrad = -1;

    // Collision Library fields ///////////////////////////////////////////
    protected boolean collisionOn = false;

    // the number of checks on collisions
    private int collisionIterations = 9;

    // the max slope to go up with sliding collision
    private boolean platformSlideMode = true;
    private double maxSlopeYunit = 0.95;

    // Your collisionGroup
    protected int collisionGroup = -1;

    /**
     *  Constructor method for all Game Objects.
     *  Automatically adds this object to the queue in GoodSky.
     */
    public GameObject(int shape, int size)
    {
        gs.addObject(this);
        setShape(shape, size, Color.WHITE);
    }
    public GameObject(int shape, int size, Color c)
    {
        gs.addObject(this);
        setShape(shape, size, c);
    }
    public GameObject(String filename)
    {
        gs.addObject(this);
        setImage(filename);
    }
    public GameObject(String filename, int width, int height, int frames)
    {
        gs.addObject(this);
        setAnimatedImage(filename, width, height, frames);
    }

    /**
     * Dispose of your Game Object the humane way. If you do not use this function
     * then your game object will not be deleted.
     */
    public final void destroy()
    {
        gs.removeObject(this);
    }

    /**
     *  Set the image of the Game Object.
     *
     * @param filename the name of the image file that you wish to set the image to.
     *  Ex. "smile.png" or "media/tree.jpg"
     */
    public final void setImage(String filename)
    {
        try {
            originalimage.clear();
            originalimage.add(ImageIO.read(new File(filename)));
            
            owidth = originalimage.get(0).getWidth();
            oheight = originalimage.get(0).getHeight();
        } catch (IOException e) {
            throw new GoodSkyException("BUMMER! I couldn't load the image file for a GameObject! Make sure you typed the file name correctly!");
        }
    }

    /**
     * Load in a string of images to make an animated image!
     * This will load in the designated number of frames from the file, this command
     * expects the frames of the image to be vertical, with the first on the top and
     * then scans down to gather the rest of the frames.
     * 
     * @param filename the filename of the image file you want to load the image from
     * @param width width of each frame in the image
     * @param height height of each frame in the image
     * @param frames number of frames
     */
    public final void setAnimatedImage(String filename, int width, int height, int frames)
    {
        try {
            originalimage.clear();

            BufferedImage fullimage = ImageIO.read(new File(filename));
            
            for (int i = 0; i < frames; i++)
            {
                if (i*height > fullimage.getHeight() || width > fullimage.getWidth())
                    throw new GoodSkyException("Error loading the animation image. Are you trying to load too many images? Or is your height or width too large?");
                
                originalimage.add(fullimage.getSubimage(0, i*height, width, height));
            }

            owidth = width;
            oheight = height;
        } catch (IOException e) {
            throw new GoodSkyException("BUMMER! I couldn't load the image file for a GameObject! Make sure you typed the file name correctly!");
        }
    }

    /**
     * If you don't want/need to use a file image for a GameObject
     * you can alternatively just set its shape.
     * 
     * @param shape the shape of the object. ex. GameObject.CIRCLE
     * @param size the size (in pixels) you want the shape to be
     */
    public final void setShape(int shape, int size, Color c)
    {
        // set the size of the new object
        owidth = size;
        oheight = size;

        originalimage.clear();
        originalimage.add(new BufferedImage(owidth, oheight, BufferedImage.TYPE_INT_ARGB));
        Graphics g = originalimage.get(0).getGraphics();

        g.setColor(c);

        if (shape == gs.TRIANGLE)
        {
            int pad = (int)Math.ceil((1-3/(2*Math.sqrt(3)))*size)/2;
            int[] xpts = {0 + pad, owidth - pad, owidth/2};
            int[] ypts = {oheight - size/4, oheight - size/4, 0};
            g.fillPolygon(xpts, ypts, 3);
        }
        else if (shape == gs.SQUARE)
            g.fillRect(0, 0, owidth, oheight);
        else
            g.fillOval(0, 0, owidth, oheight);

        g.dispose();
    }
    /**
     * Alternative version of setting the shape without color
     * @param shape Shape e.g. GameObject.CIRCLE
     * @param size Size of object in pixels
     */
    public final void setShape(int shape, int size)
    {
        setShape(shape, size, Color.WHITE);
    }

    /**
     * This protected function returns the adjusted image.
     * 
     * @return the adjusted image
     */
    protected final BufferedImage getImage()
    {
        // Make sure we have an image
        if (originalimage.isEmpty())
        {
            throw new GoodSkyException("You have not set this object's image! (it's hard to render when it doens't exist");
        }
        
        // First let's check on the frame animation if applicable
        if (animated)
        {
            if (framecounter++ >= framedelay)
            {
                framecounter = 0;
                frame = (frame+1)%originalimage.size();
            }
        }

        // used for double comparisons
        double alpha = 0.000001;

        // See if there has been a change in the scaling
        if (image == null || !(frame == oldframe &&
                 xScale > oldxScale - alpha && xScale < oldxScale + alpha &&
                 yScale > oldyScale - alpha && yScale < oldyScale + alpha &&
                 angle  > oldangle  - alpha && angle  < oldangle  + alpha))
        {
            // Create the new empty image
            image = new BufferedImage(boundwidth, boundheight, BufferedImage.TYPE_INT_ARGB);
            Graphics2D gtemp = (Graphics2D)image.getGraphics();

            // If we are at the original scale and rotation then don't do any transformations
            if (xScale == 1.0 && yScale == 1.0 && angle == 0.0)
            {
                gtemp.drawImage(originalimage.get(frame), 0, 0, null);
            }
            // If we have transformations to do, then draw them
            else
            {
                // prep the more complex transformation
                AffineTransform tx = new AffineTransform();

                // rotate the image
                tx.rotate(angle, image.getWidth()/2, image.getHeight()/2);

                // Move it to the appropriate spot of the rotation
                tx.translate((boundwidth - (owidth*xScale))/2, (boundheight - (oheight*yScale))/2);

                // First scale the image so it fits into our new box
                tx.scale(xScale, yScale);

                // Draw the final image with translations and everything
                gtemp.drawImage(originalimage.get(frame), tx, null);
            }
            
            // Store these transforms so we know next iteration
            oldxScale = xScale;
            oldyScale = yScale;
            oldangle = angle;
            oldframe = frame;

            // stop the graphics
            gtemp.dispose();
        }

        // return the image
        return image;
    }

    /**
     * Protected function updates the bounding box around the object.
     * This neat little function is used by both collision and
     * for creating my transformed BufferedImage
     */
    protected final void updateBound()
    {
        // Only do the more intense math if needed
        if (xScale == 1.0 && yScale == 1.0 && angle == 0.0)
        {
            boundwidth = owidth;
            boundheight = oheight;
        }
        else
        {
            // calculate phi and hyp needed to find the bounding box
            double modwidth  = (owidth*xScale)/2;
            double modheight = (oheight*yScale)/2;
            double phi = Math.atan(modheight/modwidth);
            double hyp = Math.sqrt((modwidth*modwidth + modheight*modheight));

            // calculate the BOUND WIDTH
            boundwidth = 2*(int)Math.ceil(Math.max(Math.abs(hyp*Math.cos(angle + phi)), Math.abs(hyp*Math.cos(angle - phi))));

            // calculate the BOUND HEIGHT
            boundheight = 2*(int)Math.ceil(Math.max(Math.abs(hyp*Math.sin(angle + phi)), Math.abs(hyp*Math.sin(angle - phi))));
        }
    }

    /**
     * This function will set the bounding box used for collision to constant values.
     * The box is centered at the very center of the image. NOTE: when you use this
     * command you will set the box to be static, the default bounding box changes as
     * you rotate the image.
     *
     * @param width the TOTAL width of the bounding box
     * @param height the TOTAL height of the bounding box
     */
    public final void setBoundingBox(int width, int height)
    {
        boxcolwidth = width;
        boxcolheight = height;
    }

    /**
     * Get the width of the object (according to the collision bounding box)
     * @return width of the object
     */
    public final int width()
    {
        return (boxcolwidth < 0 || boxcolheight < 0)?boundwidth:boxcolwidth;
    }

    /**
     * Get the height of the object (according to the collision bounding box)
     * @return height of the object
     */
    public final int height()
    {
        return (boxcolwidth < 0 || boxcolheight < 0)?boundheight:boxcolheight;
    }

    public final int radius()
    {
        return circolrad < 0 ? Math.max(boundwidth/2,boundheight/2):circolrad;
    }

    /**
     * Returns if the two objects are overlapping.
     * 
     * @param obj the object to check if you are overlapping.
     * @param type the style of collision checking to use.
     *              GameObject.COL_BOX - collision box
     *              GameObject.COL_CIRCLE - collision circles
     *              GameObject.COL_PIXEL - collision with pixel perfection
     * @return true if they are overlapping. false if they are not.
     */
    public final boolean isOverlap(GameObject obj, int type)
    {
        // difference in position
        int dx = (int)Math.abs(x - obj.x);
        int dy = (int)Math.abs(y - obj.y);

        // Collision Boxes
        if (type == gs.COL_BOX)
        {
            // get the bounding box (if we have one set
            int bx = (boxcolwidth < 0 || boxcolheight < 0)?boundwidth:boxcolwidth;
            int by = (boxcolwidth < 0 || boxcolheight < 0)?boundheight:boxcolheight;

            if (dx < bx/2 + obj.width()/2 && dy < by/2 + obj.height()/2)
                return true;
            
            return false;
        }
        // Collision Circles
        else if (type == gs.COL_CIRCLE)
        {
            double dist = Math.sqrt(dx*dx + dy*dy);

            if (dist < radius() + obj.radius())
                return true;

            return false;
        }
        else if (type == gs.COL_PIXEL)
        {
            // Check if we are within their bounding box
            if (image != null && dx < boundwidth/2 + obj.boundwidth/2 && dy < boundheight/2 + obj.boundheight/2)
            {
                // we need signs in our deltas now
                dx = (int)(x - obj.x);
                dy = (int)(y - obj.y);

                // Find our area of interest relative to this object
                int dx1 = Math.max(0, boundwidth/2 - obj.width()/2 - dx);
                int dx2 = Math.min(boundwidth, boundwidth/2 + obj.width()/2 - dx);

                int dy1 = Math.max(0, boundheight/2 - obj.height()/2 - dy);
                int dy2 = Math.min(boundheight, boundheight/2 + obj.height()/2 - dy);

                BufferedImage regioncheck1 = image.getSubimage(dx1, dy1, dx2 - dx1, dy2 - dy1);

                // Find our area of interest relative to the other object
                dx = (int)(obj.x - x);
                dy = (int)(obj.y - y);

                dx1 = Math.max(0, obj.width()/2 - boundwidth/2 - dx);
                dx2 = Math.min(obj.width(), obj.width()/2 + boundwidth/2 - dx);

                dy1 = Math.max(0, obj.height()/2 - boundheight/2 - dy);
                dy2 = Math.min(obj.height(), obj.height()/2 + boundheight/2 - dy);

                BufferedImage regioncheck2 = obj.image.getSubimage(dx1, dy1, dx2 - dx1, dy2 - dy1);

                // Check for collision now
                for (int i = 0; i < dx2 - dx1; i += 3)
                {
                    for (int j = 0; j < dy2 - dy1; j += 3)
                    {
                        if ((regioncheck1.getRGB(i, j) >> 24 & 0xff) != 0 && (regioncheck2.getRGB(i, j) >> 24 & 0xff) != 0)
                            return true;
                    }
                }
            }
            else
                return false;
        }

        // no collision
        return false;
    }

    /**
     * This is a MUCH FASTER method of finding collisions. Instead of checking for two
     * sprite images colliding, we just see if a pixel is colliding with this object.
     * @param posx the x position on the screen where you want to check for collision
     * @param posy the y position on the screen where you want to check for collision
     * @param type the style of collision checking to use.
     *              GameObject.COL_BOX - collision box
     *              GameObject.COL_CIRCLE - collision circles
     *              GameObject.COL_PIXEL - collision with pixel perfection
     * @return true if they are overlapping. false if they are not.
     */
    public final boolean isOverlapPoint(int posx, int posy, int type)
    {
        // Collision Boxes
        if (type == gs.COL_BOX)
        {
            // get the bounding box (if we have one set
            int bx = (boxcolwidth < 0 || boxcolheight < 0)?boundwidth:boxcolwidth;
            int by = (boxcolwidth < 0 || boxcolheight < 0)?boundheight:boxcolheight;

            if (posx > x - bx/2 && posx < x + bx/2
                    && posy > y - by/2 && posy < y + by/2)
                return true;

            return false;
        }
        // Collision Circles
        else if (type == gs.COL_CIRCLE)
        {
            double dx = Math.abs(x - posx);
            double dy = Math.abs(y - posy);
            double dist = Math.sqrt(dx*dx + dy*dy);

            if (dist < radius())
                return true;

            return false;
        }
        else if (type == gs.COL_PIXEL)
        {
            // Check if we are within their bounding box
            // This is a necessary comment:
            // omg... this is so much easier than two sprites overlapping xD
            if (image != null && posx > x - boundwidth/2 && posx < x + boundwidth/2
                    && posy > y - boundheight/2 && posy < y + boundheight/2)
            {
                if ((image.getRGB((int)(posx - x + boundwidth/2), (int)(posy - y + boundheight/2)) >> 24 & 0xff) != 0)
                    return true;
            }
            else
                return false;
        }

        // no collision
        return false;
    }

    /**
     * This function will turn collision on or off for this object. Collision by default is OFF.
     * You must set any object you want to be 'collidable' to on if you want to
     * use the collision commands.
     *
     * @param onOff True sets collision on; False sets collisions off
     */
    public final void setCollisionOn(boolean onOff)
    {
        collisionOn = onOff;
    }

    /**
     * Use this method to set the collision group that this object is in. Objects can currently only be in a single collision group. Use this with the group collision functions to check
     * for specific object collisions. -1 is the default group.
     * @param group the group id (any number you want)
     */
    public final void setCollisionGroup(int group)
    {
        collisionGroup = group;
    }

    /**
     * Try to move the object the amount specified in the x and y direction with a circle collision bound. If a collision
     * occurs then the object will move as close as it can to the object. This function only checks against objects in the particular group
     * NOTE: This works best with small movements (i.e. keyboard movement).
     * @param dx amount to move in the x direction
     * @param dy amount to move in the y direction
     * @param radius radius you wish to use around the object
     * @param group the group you are checking against
     * @return true if a collision occurs, false otherwise
     */
    public final boolean moveCollisionCircleGroup(double dx, double dy, double radius, int group)
    {
        // check for the case of 0, 0
        if (Math.abs(dx) < 0.000001 && Math.abs(dy) < 0.000001) return false;

        // is there a collision
        boolean isCollision = false;

        // angle of movement
        double ang = Math.atan2(dy, dx);
        double tang = ang - (Math.PI/2);
        double dang = Math.PI/(collisionIterations - 1);

        // generate the values for our circle casting
        double[] circlex = new double[collisionIterations];
        double[] circley = new double[collisionIterations];
        
        for (int i = 0; i < collisionIterations; i++)
        {
            circlex[i] = radius*Math.cos(tang);
            circley[i] = radius*Math.sin(tang);
            tang += dang;
        }
        
        // the step of each check
        double idx = Math.cos(ang);
        double idy = Math.sin(ang);

        // the 'temp' dx and dy
        double tx = idx;
        double ty = idy;

        // the furthest tx and ty we have gotten to so far
        double maxdx = dx;
        double maxdy = dy;

        // make sure we test at least one step
        if (dx >= 0 && idx > dx) idx = dx - 0.0000001;
        if (dx <= 0 && idx < dx) idx = dx + 0.0000001;
        if (dy >= 0 && idy > dy) idy = dy - 0.0000001;
        if (dy <= 0 && idy < dy) idy = dy + 0.0000001;

        // check for collision points around our final location
        // Check each game object
        for (GameObject obj : gs.base.objects)
        {
            // if this is not a collision object, then don't worry!
            if (!obj.collisionOn) continue;
            // See if it is in my group
            if (group != -1 && obj.collisionGroup != group) continue;

            // set the start position
            tx = idx;
            ty = idy;

            // iterate out from the start point to the end point
            while (!isCollision && ((dx >= 0 && tx <= maxdx) || (dx <= 0 && tx >= maxdx)) && ((dy >= 0 && ty <= maxdy) || (dy <= 0 && ty >= maxdy)))
            {
                // check each point around the circle
                for (int i = 0; i < collisionIterations; i++)
                {
                    if (obj.isOverlapPoint((int)Math.round(x + tx + circlex[i]), (int)Math.round(y + ty + circley[i]), gs.COL_PIXEL))
                    {
                        isCollision = true;
                        maxdx = tx;
                        maxdy = ty;
                        break;
                    }
                }

                // go to the next iteration of the circle cast
                tx += idx;
                ty += idy;
            }
        }

        // position the object at the last position without collision
        if ((dx >= 0 && tx > dx) || (dx <= 0 && tx < dx)) tx = dx;
        if ((dy >= 0 && ty > dy) || (dy <= 0 && ty < dy)) ty = dy;
        
        x += (isCollision ? maxdx - idx : dx);
        y += (isCollision ? maxdy - idy : dy);

        return isCollision;
    }

    /**
     * Try to move the object the amount specified in the x and y direction with a circle collision bound. If a collision
     * occurs then the object will move as close as it can to the object.
     * NOTE: This works best with small movements (i.e. keyboard movement).
     * @param dx amount to move in the x direction
     * @param dy amount to move in the y direction
     * @param radius radius you wish to use around the object
     * @return true if a collision occurs, false otherwise
     */
    public final boolean moveCollisionCircle(double dx, double dy, double radius)
    {
        return moveCollisionCircleGroup(dx, dy, radius, -1);
    }

    /**
     * Set the max slope that this object can go up when using sliding collision (between 0.0 [flat] and 1.0 [vertical])
     * @param slopeY The slope amount between 0.0 and 1.0.
     */
   public final void setMaxClimableSlop(double slopeY)
    {
        maxSlopeYunit = slopeY;
    }

    /**
     * Try to move the object the amount specified in the x and y direction with a circle collision bound. If a collision
     * occurs then the object will slide along the collision normal
     * NOTE: This works best with small movements (i.e. keyboard movement).
     * @param dx amount to move in the x direction
     * @param dy amount to move in the y direction
     * @param radius radius you wish to use around the object
     * @param group the group you want to check for collision against
     * @return true if a collision occurs, false otherwise
     */
    public final boolean slideCollisionCircleGroup(double dx, double dy, double radius, int group)
    {
        // check for the case of 0, 0
        if (Math.abs(dx) < 0.000001 && Math.abs(dy) < 0.000001) return false;

        // is there a collision?
        boolean isCollision = false;

        // angle of movement
        double ang = Math.atan2(dy, dx);
        double tang = ang - (Math.PI/2);
        double dang = Math.PI/(collisionIterations - 1); // lol dang

        // generate the values for our circle casting
        double[] checkangle = new double[collisionIterations];
        double[] circlex = new double[collisionIterations];
        double[] circley = new double[collisionIterations];

        for (int i = 0; i < collisionIterations; i++)
        {
            checkangle[i] = tang;
            circlex[i] = radius*Math.cos(tang);
            circley[i] = radius*Math.sin(tang);
            tang += dang;
        }

        // create a set to store the collision angles (it will be used to get the collision normal)
        Set<Double> angleSet = new HashSet<Double>();

        // Create the 'max' distance to check. We only need to check out to the first collision
        double maxdx = dx;
        double maxdy = dy;

        // the step of each check (note the delta value is 1)
        double idx = Math.cos(ang);
        double idy = Math.sin(ang);

        // the 'temp' dx and dy, the ones that we increment along our trip
        double tx = idx;
        double ty = idy;

        // make sure we test at least one step
        if (dx >= 0 && idx > dx) idx = dx - 0.0000001;
        if (dx <= 0 && idx < dx) idx = dx + 0.0000001;
        if (dy >= 0 && idy > dy) idy = dy - 0.0000001;
        if (dy <= 0 && idy < dy) idy = dy + 0.0000001;

        // check for collision points around our final location
        // Check each game object
        for (GameObject obj : gs.base.objects)
        {
            // if this is not a collision object, then don't worry!
            if (!obj.collisionOn) continue;
            // See if it is in my group
            if (group != -1 && obj.collisionGroup != group) continue;

            // reset the start position
            tx = idx;
            ty = idy;

            // iterate out from the start point to the end point
            while (!isCollision && ((dx >= 0 && tx <= maxdx + 0.0000001) || (dx <= 0 && tx >= maxdx - 0.0000001)) && ((dy >= 0 && ty <= maxdy + 0.0000001) || (dy <= 0 && ty >= maxdy - 0.0000001)))
            {
                // check each point around the circle
                for (int i = 0; i < collisionIterations; i++)
                {
                    // collision at a certain point
                    if (obj.isOverlapPoint((int)Math.round(x + tx + circlex[i]), (int)Math.round(y + ty + circley[i]), gs.COL_PIXEL))
                    {
                        // set maxdx and maxdy no further than what we are at and reset the angleSet
                        // This reset only needs to happen if we ARE BEFORE THE CURRENT MAXDX and MAXDY
                        if ((tx == 0 || (dx >= 0 && tx < maxdx) || (dx <= 0 && tx > maxdx)) && (ty == 0 || (dy >= 0 && ty < maxdy) || (dy <= 0 && ty > maxdy)))
                        {
                            maxdx = tx;
                            maxdy = dy;
                            angleSet.clear();
                        }

                        // Add this collision to the angle Set
                        angleSet.add(checkangle[i]);

                        // Oh btw: there is a collision
                        isCollision = true;
                    }
                }

                // go to the next iteration of the circle cast
                tx += idx;
                ty += idy;
            }
        }

        // Evaluate the normal
        // Then slide! (if necessary)
        double normal;

        if (isCollision)
        {
            // Add up all the values (@-@ Hash Set!!!)
            double sum = 0;
            Iterator<Double> e = angleSet.iterator();
            while (e.hasNext()) sum += e.next();

            // get the average, that is my normal :P
            normal = sum/angleSet.size();

            // If we are doing a platformer, ignore collisions above about waist level
            if (platformSlideMode)
            {
                if ((normal > Math.PI && normal < Math.PI * 2) ||(normal > Math.PI * -1 && normal < 0))
                    normal -= Math.PI;
                else
                    normal -= Math.PI / 2 * (dx < 0 ? -1 : 1);
            }
            else
            {
                // TODO: in top-down mode change the normal to go with the velocity
                normal -= Math.PI / 2 * (dx < 0 ? -1 : 1);
            }

            // see how much movement we have left
            // Calculate this as a magnitude
            double resultMagnitude = Math.sqrt(Math.pow(dx + idx - maxdx, 2) + Math.pow(dy + idy - maxdy, 2));

            // calculate the unit vector in the direction of the normal
            double unitnormx = Math.cos(normal);
            double unitnormy = Math.sin(normal);

            // cap the movement!
            if ((dx >= 0 && tx > maxdx) || (dx <= 0 && tx < maxdx)) tx = maxdx;
            if ((dy >= 0 && ty > maxdy) || (dy <= 0 && ty < maxdy)) ty = maxdy;

            // Max sure that the movement does not exceed the max Y Slope WHEN IN PLATFORM MODE
            if (platformSlideMode && unitnormy < maxSlopeYunit * -1)
            {
                unitnormx = 0; unitnormy = 0;
            }

            // dot product magic to get the sliding movement
            tx += (resultMagnitude * unitnormx);
            ty += (resultMagnitude * unitnormy);

            //System.out.println("tx: " + tx + " dx: " + dx + " maxdx: " + maxdx);
            //System.out.println(" Collisions: " + angleSet.size() +  " normal: " + normal + " unit x: " + unitnormx + " unit y: " + unitnormy + " result Magnitude: " + resultMagnitude);
        }
        else
        {
            // cap the movement!
            if ((dx >= 0 && tx > dx) || (dx <= 0 && tx < dx)) tx = dx;
            if ((dy >= 0 && ty > dy) || (dy <= 0 && ty < dy)) ty = dy;
        }

        x += (isCollision ? tx - idx : tx);
        y += (isCollision ? ty - idy : ty);

        return isCollision;
    }

    /**
     * Try to move the object the amount specified in the x and y direction with a circle collision bound. If a collision
     * occurs then the object will slide along the collision normal
     * NOTE: This works best with small movements (i.e. keyboard movement).
     * @param dx amount to move in the x direction
     * @param dy amount to move in the y direction
     * @param radius radius you wish to use around the object
     * @return true if a collision occurs, false otherwise
     */
    public final boolean slideCollisionCircle(double dx, double dy, double radius)
    {
        return slideCollisionCircleGroup(dx, dy, radius, -1);
    }

    /**
     * Returns if there is a collision at the hypothetical dx and dy without actually moving the object.
     * @param dx amount to move in the x direction
     * @param dy amount to move in the y direction
     * @param radius radius you wish to use around the object
     * @param group the group you wish to check for collisions against
     * @return true if there is a collision, false otherwise
     */
    public final boolean castCollisionCircleGroup(double dx, double dy, double radius, int group)
    {
        // check for the case of 0, 0
        if (Math.abs(dx) < 0.000001 && Math.abs(dy) < 0.000001) return false;

        // is there a collision
        boolean isCollision = false;

        // angle of movement
        double ang = Math.atan2(dy, dx);
        double tang = ang - (Math.PI/2);
        double dang = Math.PI/collisionIterations;

        // generate the values for our circle casting
        double[] circlex = new double[collisionIterations];
        double[] circley = new double[collisionIterations];
        for (int i = 0; i < collisionIterations; i++)
        {
            circlex[i] = radius*Math.cos(tang);
            circley[i] = radius*Math.sin(tang);
            tang += dang;
        }

        // the step of each check
        double idx = Math.cos(ang);
        double idy = Math.sin(ang);

        // the 'temp' dx and dy
        double tx = 0;
        double ty = 0;

        // the furthest tx and ty we have gotten to so far
        double maxdx = dx;
        double maxdy = dy;

        // make sure we test at least one step
        if (dx >= 0 && idx > dx) idx = dx - 0.0000001;
        if (dx <= 0 && idx < dx) idx = dx + 0.0000001;
        if (dy >= 0 && idy > dy) idy = dy - 0.0000001;
        if (dy <= 0 && idy < dy) idy = dy + 0.0000001;

        // check for collision points around our final location
        // Check each game object
        for (GameObject obj : gs.base.objects)
        {
            // if this is not a collision object, then don't worry!
            if (!obj.collisionOn) continue;
            // See if it is in my group
            if (group != -1 && obj.collisionGroup != group) continue;

            // set the start position
            tx = idx;
            ty = idy;

            // check for collision points around our final location
            while (((dx >= 0 && tx <= maxdx) || (dx <= 0 && tx >= maxdx)) && ((dy >= 0 && ty <= maxdy) || (dy <= 0 && ty >= maxdy)))
            {
                for (int i = 0; i < collisionIterations; i++)
                {
                    if (obj.isOverlapPoint((int)Math.round(x + tx + circlex[i]), (int)Math.round(y + ty + circley[i]), gs.COL_PIXEL))
                    {
                        isCollision = true;
                        maxdx = tx;
                        maxdy = ty;
                        break;
                    }
                }

                // go to the next iteration of the circle cast
                tx += idx;
                ty += idy;
                }
        }

        return isCollision;
    }

    /**
     * Returns if there is a collision at the hypothetical dx and dy without actually moving the object.
     * @param dx amount to move in the x direction
     * @param dy amount to move in the y direction
     * @param radius radius you wish to use around the object
     * @return true if there is a collision, false otherwise
     */
    public final boolean castCollisionCircle(double dx, double dy, double radius)
    {
        return castCollisionCircleGroup(dx, dy, radius, -1);
    }

    /**
     * Casts a line out of the object from the origin to the position (x + dx, y + dy), returns if there is a collision.
     * If there is a collision it will return the distance to the collision. If there is no collision it will return a negative number.
     * This is a good method to use for checking if projectiles hit.
     * @param dx amount to move in the x direction
     * @param dy amount to move in the y direction
     * @param group the group you wish to check against
     * @return the distance to the collision, or a negative number if there is no collision
     */
    public final double castCollisionLineGroup(double dx, double dy, int group)
    {
        // check for the case of 0, 0
        if (Math.abs(dx) < 0.000001 && Math.abs(dy) < 0.000001) return -1;

        // is there a collision
        double isCollision = -1;

        // angle of movement
        double ang = Math.atan2(dy, dx);
        double tang = ang - (Math.PI/2);
        double dang = Math.PI/collisionIterations;

        // the step of each check
        double idx = Math.cos(ang);
        double idy = Math.sin(ang);

        // the 'temp' dx and dy
        double tx = 0;
        double ty = 0;

        // the furthest tx and ty we have gotten to so far
        double maxdx = dx;
        double maxdy = dy;

        // make sure we test at least one step
        if (dx >= 0 && idx > dx) idx = dx - 0.0000001;
        if (dx <= 0 && idx < dx) idx = dx + 0.0000001;
        if (dy >= 0 && idy > dy) idy = dy - 0.0000001;
        if (dy <= 0 && idy < dy) idy = dy + 0.0000001;

        // check for collision points around our final location
        // Check each game object
        for (GameObject obj : gs.base.objects)
        {
            // if this is not a collision object, then don't worry!
            if (!obj.collisionOn) continue;
            // See if it is in my group
            if (group != -1 && obj.collisionGroup != group) continue;

            // set the start position
            tx = idx;
            ty = idy;

            // check for collision points around our final location
            while (((dx >= 0 && tx <= maxdx) || (dx <= 0 && tx >= maxdx)) && ((dy >= 0 && ty <= maxdy) || (dy <= 0 && ty >= maxdy)))
            {
                if (obj.isOverlapPoint((int)Math.round(x + tx), (int)Math.round(y + ty), gs.COL_PIXEL))
                {
                    isCollision = Math.sqrt(tx*tx + ty*ty);
                    maxdx = tx;
                    maxdy = ty;
                    break;
                }

                // go to the next iteration of the circle cast
                tx += idx;
                ty += idy;
            }
        }

        return isCollision;
    }

    /**
     * Casts a line out of the object from the origin to the position (x + dx, y + dy), returns if there is a collision.
     * If there is a collision it will return the distance to the collision. If there is no collision it will return a negative number.
     * This is a good method to use for checking if projectiles hit.
     * @param dx amount to move in the x direction
     * @param dy amount to move in the y direction
     * @return the distance to the collision, or a negative number if there is no collision
     */
    public final double castCollisionLine(double dx, double dy)
    {
        return castCollisionLineGroup(dx, dy, -1);
    }

    /**
     * Scale the GameObject's image. 1.0 is 100% (regular size). So twice the size would be 2.0.
     * @param x xScale
     * @param y yScale
     */
    public final void scale(double x, double y)
    {
        xScale = x;
        yScale = y;
    }
    public final void scale(double s)
    {
        xScale = s;
        yScale = s;
    }

    /**
     * Rotate the image by the amount 'ang' in degrees
     * 
     * @param ang the angle to rotate the object to
     */
    public final void rotate(double ang)
    {
        angle = ang;

        // clip the angle to be between 0 and 360
        while (angle < 0.0 || angle > 360.0)
        {
            if (angle < 0.0) angle += 360.0;
            else    angle -= 360.0;
        }
            
        angle = Math.toRadians(angle);
    }

    /**
     * Set the position of the game object
     * NOTE: (0,0) is in the top left of the screen.
     * 
     * @param x x position of the game object.
     * @param y y position of the game object.
     */
    public final void position(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    /**
     * Set the current frame of the image.
     * @param frame the frame to set the image to
     */
    public final void setFrame(int frame)
    {
        this.frame = frame;

        if (this.frame >= originalimage.size())
            this.frame = originalimage.size() - 1;
    }

    /**
     * Turn animation on or off on a particular object.
     * @param onoff true if you want it to animate. false if you want it to stop animating.
     */
    public final void animate(boolean onoff)
    {
        animated = onoff;
    }

    /**
     * Set the speed of the animation. The argument 'framedelay' measures the number of seconds
     * between each frame. So if you want a half second delay between each frame switch put in 0.5.
     *
     * @param framedelay The delay (in seconds) between each frame switch when animated.
     */
    public final void setAnimationSpeed(double framedelay)
    {
        this.framedelay = (int)(framedelay*(gs.getFPS() < 1?40:gs.getFPS()));
    }

    /**
     * Get the frame that the image is currently on.
     * @return the frame we are on.
     */
    public final int getFrame()
    {
        return frame;
    }

    /**
     * Get the X Scale of the object
     * @return X Scale (e.g. 100% scale is 1.0, twice the size would be 2.0)
     */
    public final double getXScale()
    {
        return xScale;
    }

    /**
     * Get the Y Scale of the object
     * @return Y Scale (e.g. 100% scale is 1.0, twice the size would be 2.0)
     */
    public final double getYScale()
    {
        return yScale;
    }

    /**
     * Get the angle the object is currently facing in degrees
     * @return angle in degrees
     */
    public final double getAngle()
    {
        return Math.toDegrees(angle);
    }

    /**
     * Set the render depth. Lower depth GameObjects will be IN FRONT.
     * default depth is 10
     * @param z: the depth for the game object
     */
    public final void setDepth(int z)
    {
        depth = z;

        Collections.sort(gs.base.objects);
    }

    /**
     * Compares the depth of two GameObjects. This is used internally, not necessary for users.
     * @param o
     * @return positive if the comparator object goes before 'o', negative otherwise. 0 for a tie.
     */
    public final int compareTo(GameObject o) {
        return o.depth - depth;
    }

    public String toString()
    {
        return "Game Object: width-"+owidth+" height-"+oheight;
    }
}
