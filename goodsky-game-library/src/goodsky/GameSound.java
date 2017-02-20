/*
 * This class holds all the necessary information for a sound to play.
 *
 * Much of this class is thanks to David Flanagan and William Bittle. Thank you for your examples.
 */
package goodsky;

import java.io.File;
import javax.sound.sampled.*;
import javax.sound.midi.*;

/**
 *
 * @author Skyler Goodell
 */
public class GameSound
{
    // A flag saying if this is a midi or not
    private boolean ismidi;

    // MIDI file fields
    private Sequencer sequencer = null;
    private Receiver receiver = null;

    // streaming clip fields
    private Clip myclip = null;

    /**
     * The constructor of a GameSound requires you to simply put in the address of the
     * sound you wish to load. It will automatically detect the sound type and load
     * the correct systems from there.
     *
     * Supported sound types include .wav and .mid
     * NOTE: mp3 are not supported as of now!
     * 
     * @param filename address of the sound you wish to load.
     */
    public GameSound(String filename)
    {
        // Attempt to load in the new sound
        File file = new File(filename);

        // See if this new file is a midi file or not
        try {
            MidiSystem.getMidiFileFormat(file);
            ismidi = true;
        }
        catch (InvalidMidiDataException e)
        {
            ismidi = false;
        }
        catch (Exception  e)
        {
            throw new GoodSkyException("OH NOES! I had a problem opening up your sound file. Check on that.");
        }

        // Load the file and get it ready to play when we need it to
        if (ismidi)
        {
            try {
                // The sequencer sends the events to the Synthesizer at the right time
                sequencer = MidiSystem.getSequencer(false);
                receiver = MidiSystem.getReceiver();

                sequencer.open();
                sequencer.getTransmitter().setReceiver(receiver);

                // The sequencer and the synthesizer apparently may not be connected automatically
                // So we will explicitely link them
                Sequence sequence = MidiSystem.getSequence(file);
                sequencer.setSequence(sequence);
            }
            catch (Exception e)
            {
                throw new GoodSkyException("OH NOES! I had a problem opening up your sound file. Tell Skyler this exception popped up in MIDI phase 2.");
            }
        }
        // load the streaming clip
        else
        {
            try {
                AudioInputStream audioinput = AudioSystem.getAudioInputStream(file);

                try {
                    DataLine.Info info = new DataLine.Info(Clip.class, audioinput.getFormat());
                    myclip = (Clip) AudioSystem.getLine(info);
                    myclip.open(audioinput);
                }
                finally {
                    audioinput.close();
                }
            }
            catch (Exception e)
            {
                throw new GoodSkyException("OH NOES! I had a problem opening up your sound file. Tell Skyler this exception popped up in streaming phase 2. (please remember that .mp3s are not supported yet)");
            }
        }
    }

    /**
     * Start playing the audio clip
     */
    public final void play()
    {
        // If the clip is at the end and we call play, then reset the clip
        if (ismidi && sequencer.getMicrosecondPosition() >= sequencer.getMicrosecondLength() - 1)
            reset();
        else if (!ismidi && myclip.getMicrosecondPosition() >= myclip.getMicrosecondLength() - 1)
            reset();

        // play the clip
        if (ismidi)
            sequencer.start();
        else
            myclip.start();
    }

    /**
     * Stop playing the audio clip
     */
    public final void stop()
    {
        if (ismidi)
            sequencer.stop();
        else
            myclip.stop();
    }

    /**
     * Set the clip playback to the beginning.
     */
    public final void reset()
    {
        if (ismidi)
            sequencer.setTickPosition(0);
        else
            myclip.setMicrosecondPosition(0);
    }

    /**
     * See if the clip is still playing. If the clip has completed playing please consider either looping it or DELETEing it!
     * @return true if the clip is playing. false if the clip is finished playing.
     */
    public final boolean isPlaying()
    {
        if (ismidi && sequencer.getMicrosecondPosition() >= sequencer.getMicrosecondLength() - 1)
            return false;
        else if (!ismidi && myclip.getMicrosecondPosition() >= myclip.getMicrosecondLength() - 1)
            return false;
        
        return true;
    }

    public final void setVolume(int vol)
    {
        if (ismidi)
        {
            try {
                Thread.sleep(10);
                ShortMessage volMessage = new ShortMessage();
                for (int i = 0; i < 16; i++) {
                  try {
                    volMessage.setMessage(ShortMessage.CONTROL_CHANGE, i, 7, vol);
                  } catch (InvalidMidiDataException e) {}
                  receiver.send(volMessage, -1);
                }

            }
            catch (Exception e)
            {
                throw new GoodSkyException("Problem setting the MIDI volume");
            }
        }
        else
        {
            FloatControl gainControl = (FloatControl) myclip.getControl(FloatControl.Type.MASTER_GAIN);

            // translate the 0-127 scale to decibels
            float dvol = gainControl.getMaximum() - gainControl.getMinimum();
            float volume = gainControl.getMinimum() + dvol/2 + (dvol/2)*((float)vol/127.0f);

            // set the master volume on the sound
            gainControl.setValue(volume);
        }
    }

    /**
     * Free the resources dedicated to this sound (you must do this!!!)
     */
    public final void delete()
    {
        if (ismidi)
            sequencer.close();
        else
            myclip.close();
    }
}
