using UnityEngine;

/// <summary>
/// Really simple class that encapsulates the functionality
/// of a timer. Starts on construction, you can Restart(),
/// Pause(), Play(), get Elapsed() time in seconds, and see if
/// it IsPaused().
/// </summary>
public class Timer 
{
    public void Restart()
    {
        m_start = Time.time;
        m_pause = 0;
        m_paused = false;
    }

    public float Elapsed()
    {
        if (m_paused) return m_pause - m_start;

        return Time.time - m_start;
    }

    public void Pause()
    {
        m_pause = Time.time;
        m_paused = true;
    }

    public void Unpause()
    {
        m_start += Time.time - m_pause;
        m_pause = 0;
        m_paused = false;
    }

    public bool IsPaused()
    {
        return m_paused;
    }

    public Timer()
    {
        Restart();
       
    }

    //----Nothing public down here!
    protected bool m_paused;
    protected float m_start;
    protected float m_pause;
}
