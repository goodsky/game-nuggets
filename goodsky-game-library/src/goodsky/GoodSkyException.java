package goodsky;

// *********************************************************
// A nested class to hold information about exceptions
// *********************************************************
public class GoodSkyException extends RuntimeException
{
    // The error message
    private String msg;

    public GoodSkyException(String msg)
    {
        super(msg);
        this.msg = msg;
    }

    public String getError()
    {
        return msg;
    }
}
