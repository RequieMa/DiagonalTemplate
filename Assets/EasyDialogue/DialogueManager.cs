using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using EasyDialogue;
using StarterAssets;

namespace EasyDialogue.Samples
{
    [RequireComponent(typeof(Canvas))]
    public class DialogueManager : MonoBehaviour
    {
        #region Attributes and Properties
        [SerializeField]
        private TMP_Text textBox;
        [SerializeField]
        private TMP_Text characterName;
        [SerializeField]
        private Image characterImage;
        [SerializeField]
        private GameObject[] playerChoiceBottons;
        [SerializeField]
        private EasyDialogueGraph graphToPlay;
        [SerializeField]
        bool startWithOverrideCharacter;
        [SerializeField, Tooltip("This will only be used if \"startWithOverrideCharacter\" is ticked on")]
        Character overrideCharacter;
        [SerializeField]
        CheckTrigger checkTrigger;
        [SerializeField]
        ThirdPersonController thirdPersonController;

        private readonly List<TMP_Text> playerChoices = new();
        private readonly item_data[] items = new item_data[3]
        {
            new("coffe", "coffee", ItemType.Consumable),
            new("sword", "sword", ItemType.Equippable),
            new("watter_ballon", "water ballon", ItemType.Throwable)
        };
        private EasyDialogueGraph currentGraph;
        private EasyDialogueManager dialogueManager;
        private Canvas myCanvas;

        public bool HasDialogueGraph { get { return currentGraph != null; } }
        #endregion

        #region Unity Functions
        private void Start()
        {
            myCanvas = GetComponent<Canvas>();
            dialogueManager = FindObjectOfType<EasyDialogueManager>();
            dialogueManager.OnDialogueStarted += (_graph, _dl) => { Debug.Log($"{_dl.text} was said by {_dl.character}"); };
            dialogueManager.OnDialogueProgressed += (_graph, _line) => { Debug.Log($"{_line.text} was said by {_line.character}"); };
            dialogueManager.OnDialogueEnded += _graph => Debug.Log($"Dialogue ended on graph {_graph.name}");
            for (int i = 0; i < playerChoiceBottons.Length; ++i)
            {
                playerChoices.Add(playerChoiceBottons[i].GetComponentInChildren<TMP_Text>());
            }
            InitializeDialogue();
        }

        void Update()
        {
            //Next Dialogue (Also starts an encounter)
            if (Input.GetKeyDown(KeyCode.N))
            {
                if (HasDialogueGraph)
                {
                    //Progress Dialogue if no player response, or select response 1.
                    GetNextDialogue();
                }
                else
                {
                    if (checkTrigger.CanReact)
                    {
                        StartDialogueEncounter(ref graphToPlay);
                    }
                }
            }
            //Select Player Response 1
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GetNextDialogue(0);
            }
            //Select Player Response 2
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GetNextDialogue(1);
            }
            //Select Player Response 3
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GetNextDialogue(2);
            }
            //Quit dialogue
            if (Input.GetKeyDown(KeyCode.Q) && HasDialogueGraph)
            {
                EndDialogue();
            }
        }
        #endregion

        #region Main Functionality
        /// <summary>
        /// Called to start the dialogue with the given graph.
        /// </summary>
        /// <param name="_dialogueGraph"></param>
        public void StartDialogueEncounter(ref EasyDialogueGraph _dialogueGraph)
        {
            currentGraph = _dialogueGraph;
            currentGraph.localGraphContext = SetupCustomGraphContext();
            if (startWithOverrideCharacter)
            {
                currentGraph.AddOverrideCharacter(ref overrideCharacter);
            }
            dialogue_line dialogue = dialogueManager.StartDialogueEncounter(ref _dialogueGraph);
            DisplayDialogue(ref dialogue);
        }

        /// <summary>
        /// Get's the next dialogue, from the 1,2,3 inputs as well as button clicks.
        /// </summary>
        /// <param name="_choiceIndex"></param>
        public void GetNextDialogue(int _choiceIndex = 0)
        {
            if (!HasDialogueGraph) return;
            if (dialogueManager.GetNextDialogue(ref currentGraph, out dialogue_line dialogue, (ushort)_choiceIndex))
            {
                DisplayDialogue(ref dialogue);
            }
            else
            {
                thirdPersonController.enabled = true;
                // thirdPersonController.CursorInput.cursorLocked = true;
                thirdPersonController.CursorInput.cursorInputForLook = true;
                InitializeDialogue();
            }
        }

        /// <summary>
        /// Abruptly ends or kills the current dialogue session.
        /// </summary>
        public void EndDialogue()
        {
            if (dialogueManager.EndDialogueEncounter(ref currentGraph))
            {
                InitializeDialogue();
            }
        }

        #endregion

        //All of the following functions are for the presentation and setting of UI.
        #region Helper Functions

        private CustomGraphContext SetupCustomGraphContext()
        {
            CustomGraphContext cgc = new CustomGraphContext();
            cgc.heldItem = items[Random.Range(0, items.Length)];
            return cgc;
        }


        private void InitializeDialogue()
        {
            characterImage.sprite = null;
            currentGraph = null;
            textBox.text = "Initialized text box, please start a dialogue";
            characterName.text = "The mystical asset creator";
            myCanvas.enabled = false;
            HidePlayerResponses();
        }

        private void DisplayDialogue(ref dialogue_line _dialogue)
        {
            ShowCharacterDialogue(_dialogue.character, _dialogue.text);
            if (_dialogue.HasPlayerResponses())
            {
                ShowPlayerResponses(_dialogue.playerResponces);
            }
            else
            {
                // thirdPersonController.CursorInput.cursorLocked = false;
                thirdPersonController.CursorInput.cursorInputForLook = false;
                thirdPersonController.enabled = false;
                HidePlayerResponses();
            }
        }

        private void ShowCharacterDialogue(Character _character, string _text)
        {
            myCanvas.enabled = true;
            textBox.text = _text;
            characterName.text = _character.displayName;
            characterImage.sprite = _character.portrait;
        }

        private void HidePlayerResponses()
        {
            foreach (var playerChoice in playerChoices)
            {
                playerChoice.text = "No option avalible";
            }
            foreach (var button in playerChoiceBottons)
            {
                button.SetActive(false);
            }
        }

        private void ShowPlayerResponses(string[] _responses)
        {
            for (int i = 0; i < _responses.Length; ++i)
            {
                playerChoices[i].text = _responses[i];
                playerChoiceBottons[i].SetActive(true);
            }
        }
        #endregion
    }
}