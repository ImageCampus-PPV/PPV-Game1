using System;

[Serializable]
public class PlayerSettings
{
    public float speed;
    public float jumpForce;
}

public class PlayerModel
{
    public PlayerSettings Settings { get; private set; }
    public float MoveXInputDir { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool JumpRequested { get; private set; }

    public PlayerModel(PlayerSettings settings)
    {
        Settings = settings;
    }

    public void SetMoveXInput(float input)
    {
        MoveXInputDir = Math.Clamp(input, -1f, 1f);
    }

    public void SetGrounded(bool grounded)
    {
        IsGrounded = grounded;
    }

    public void RequestJump()
    {
        if (IsGrounded)
        {
            JumpRequested = true;
        }
    }

    public void ConsumeJump()
    {
        JumpRequested = false;
    }
}