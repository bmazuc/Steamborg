using UnityEngine;
using XInputDotNetPure; // Required in C#

public class Controller : MonoBehaviour
{
	[SerializeField, Range(1, 2)]
	private int playerNumber = 1;
    public int PlayerNumber { set { playerNumber = value; } }

    private Player player;

    bool playerIndexSet = false;
    PlayerIndex playerIndex;
    GamePadState state;
    GamePadState prevState;

    private string horizontalInput = "Horizontal_P";
    private string verticalInput = "Vertical_P";
    //private string jumpInput = "Jump_P";
    //private string specialInput = "SpecialAttack_P";
    //private string lightInput = "LightAttack_P";
    //private string strongInput = "StrongAttack_P";
    //private string dodgeInput = "Dodge_P";
    //private string switchInput = "Switch_P";
    private string pauseInput = "Pause_P";
    //   private string joinInput = "Join_P";

    private bool dodgePressed = false;
	private bool switchPressed = false;

    private bool isLock = false;

    private bool test = true;

	protected void Awake()
	{
        player = GetComponent<Player>();

        horizontalInput += playerNumber.ToString();
        verticalInput += playerNumber.ToString();
        //jumpInput += playerNumber.ToString();
        //specialInput += playerNumber.ToString();
        //lightInput += playerNumber.ToString();
        //strongInput += playerNumber.ToString();
        //dodgeInput += playerNumber.ToString();
        //pauseInput += playerNumber.ToString();
        //joinInput += playerNumber.ToString();
    }

    public void ChangeControllerPlayer(int playerNumber_)
    {
        playerNumber = playerNumber_;

        PlayerIndex testPlayerIndex = (PlayerIndex)(playerNumber - 1);
        GamePadState testState = GamePad.GetState(testPlayerIndex);

        if (testState.IsConnected)
        {
            Debug.Log(string.Format("GamePad found {0}", testPlayerIndex));
            playerIndex = testPlayerIndex;
            playerIndexSet = true;
        }

        horizontalInput = horizontalInput.Substring(0, horizontalInput.Length - 1) + playerNumber.ToString();
        verticalInput = verticalInput.Substring(0, verticalInput.Length - 1) + playerNumber.ToString();
        //jumpInput = jumpInput.Substring(0, jumpInput.Length - 1) + playerNumber.ToString();
        //specialInput = specialInput.Substring(0, specialInput.Length - 1) + playerNumber.ToString();
        //lightInput = lightInput.Substring(0, lightInput.Length - 1) + playerNumber.ToString();
        //strongInput = strongInput.Substring(0, strongInput.Length - 1) + playerNumber.ToString();
        //dodgeInput = dodgeInput.Substring(0, dodgeInput.Length - 1) + playerNumber.ToString();
        //pauseInput = pauseInput.Substring(0, pauseInput.Length - 1) + playerNumber.ToString();
        //joinInput = joinInput.Substring(0, joinInput.Length - 1) + playerNumber.ToString();
    }

    public void Lock(bool state)
    {
        isLock = state;
    }

    protected void Update()
    {
        CheckState();

        if (!isLock && !player.IsDead && player.CCMgr.CurrCC == CrowdControl.CONTROL_TYPE.None)
        {
            player.Move(new Vector3(state.ThumbSticks.Left.X, 0f, state.ThumbSticks.Left.Y).normalized);

            if (prevState.Buttons.A == ButtonState.Released && state.Buttons.A == ButtonState.Pressed)
            {
                player.Jump();
            }

            if (prevState.Buttons.B == ButtonState.Released && state.Buttons.B == ButtonState.Pressed)
            {
                player.SpecialAttack();
            }

            if (prevState.Buttons.B == ButtonState.Pressed && state.Buttons.B == ButtonState.Released)
            {
                 player.StopMorphAttack();
            }

            if (prevState.Buttons.X == ButtonState.Released && state.Buttons.X == ButtonState.Pressed)
            {
                player.LightAttack();
            }

            if (prevState.Buttons.Y == ButtonState.Released && state.Buttons.Y == ButtonState.Pressed)
            {
                player.StrongAttack();
            }

            if (state.Triggers.Right > 0.8f && !dodgePressed)
            {
                dodgePressed = true;
                player.Dodge();
            }
            else if (state.Triggers.Right < 0.7f && dodgePressed)
            {
                dodgePressed = false;
            }
        }
        else
            player.Move(Vector3.zero);

        //if (prevState.Buttons.Start == ButtonState.Released && state.Buttons.Start == ButtonState.Pressed)
        //    GameManager.Instance.PauseManager.Switch(playerNumber);
            
    }


    public void CheckState()
    {
        prevState = state;
        state = GamePad.GetState(playerIndex);
    }
}
