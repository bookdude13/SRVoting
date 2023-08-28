﻿using Il2Cpp;
using SRModCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SRVoting.UI
{
    public class VoteDirectionComponent
    {
        private SRLogger logger;
        private UnityAction onArrowUse;
        private GameObject arrow;
        private Il2Cpp.SynthUIButton synthButton;
        private Il2CppTMPro.TMP_Text countText;

        public string ArrowName { get; private set; }
        public bool IsUiCreated { get; private set; } = false;


        public VoteDirectionComponent(SRLogger logger, string arrowName, UnityAction onArrowUse)
        {
            this.logger = logger;
            ArrowName = arrowName;
            this.onArrowUse = onArrowUse;
        }

        public void DisableEvents()
        {
            synthButton?.WhenClicked?.RemoveAllListeners();
        }

        public void UpdateUI(bool isActive, bool isMyVote, string text)
        {
            countText.SetText(text);

            if (isMyVote)
            {
                countText.fontStyle = Il2CppTMPro.FontStyles.Underline;
                countText.color = Color.white;
            }
            else
            {
                countText.fontStyle = Il2CppTMPro.FontStyles.Normal;
                countText.color = new Color(0.4f, 0.4f, 0.4f, 1.0f);
            }

            var vrtkHelper = arrow.GetComponent<Il2CppSynth.Utils.VRTKButtonHelper>();
            if (isActive)
            {
                synthButton?.WhenClicked?.RemoveAllListeners();
                synthButton?.WhenClicked?.AddListener(onArrowUse);
                vrtkHelper.SetActive();
            }
            else
            {
                synthButton?.WhenClicked?.RemoveAllListeners();
                vrtkHelper.SetInactive();
            }
        }

        public void CreateUIForHorizontal(
            Transform parent,
            float offsetX,
            Il2CppTMPro.TextAlignmentOptions textAlignment,
            Transform arrowToClone,
            GameObject textReference
        )
        {
            if (IsUiCreated)
            {
                return;
            }

            IsUiCreated = true;

            var voteContainer = new GameObject("srvoting_container");
            voteContainer.transform.SetParent(parent, false);
            voteContainer.transform.localPosition = Vector3.zero;
            voteContainer.transform.localRotation = parent.localRotation;

            arrow = CreateVoteArrow(voteContainer.transform, arrowToClone);
            countText = CreateVoteCountText(voteContainer.transform, arrow, textReference);

            arrow.transform.localPosition += new Vector3(offsetX, 0.0f, 0.0f);
            countText.transform.localPosition += new Vector3(offsetX * 2.0f, 0.0f, 0.0f);
            countText.alignment = textAlignment;
        }

        public void CreateUIForVertical(
            Transform parent,
            Transform leftSideReference,
            Transform rightOffsetReference,
            Transform arrowToClone,
            GameObject textReference
        )
        {
            if (IsUiCreated)
            {
                return;
            }

            IsUiCreated = true;

            var voteContainer = new GameObject("srvoting_container");
            voteContainer.transform.SetParent(parent, false);
            voteContainer.transform.localPosition = leftSideReference.localPosition + rightOffsetReference.localPosition + new Vector3(2.0f, 0.0f, 0.0f);
            voteContainer.transform.localRotation = leftSideReference.localRotation;

            arrow = CreateVoteArrow(voteContainer.transform, arrowToClone);
            countText = CreateVoteCountText(voteContainer.transform, arrow, textReference);
            countText.transform.localPosition += new Vector3(1.2f, 0.0f, 0.0f);
        }

        private GameObject CreateVoteArrow(Transform voteContainer, Transform arrowToClone)
        {
            // Clone arrow used to actually vote
            var voteArrow = GameObject.Instantiate(arrowToClone, voteContainer.transform);
            voteArrow.name = ArrowName;
            voteArrow.localPosition = Vector3.zero;
            voteArrow.localEulerAngles = arrowToClone.localEulerAngles + new Vector3(0f, 0f, 90f);

            // Replace button event
            var synthButton = voteArrow.GetComponent<SynthUIButton>();
            synthButton.WhenClicked.RemoveAllListeners();

            // 2 persistent listeners not removed by RemoveAllListeners() exist
            // See https://forum.unity.com/threads/documentation-unityevent-removealllisteners-only-removes-non-persistent-listeners.341796/
            // After trial and error, the one at index 0 controls volume still and needs to be disabled.
            // The one at index 1 still needs to stick around to handle new events
            //synthButton.WhenClicked.SetPersistentListenerState(0, UnityEventCallState.Off);

            voteArrow.gameObject.SetActive(true);
            synthButton.WhenClicked.AddListener(onArrowUse);

            logger.Debug($"Arrow added");
            return voteArrow.gameObject;
        }

        private Il2CppTMPro.TMP_Text CreateVoteCountText(Transform voteContainer, GameObject voteArrow, GameObject textReference)
        {
            var voteCountText = GameObject.Instantiate(textReference, voteContainer);
            voteCountText.name = voteArrow.name + "_text";
            voteCountText.transform.localPosition = voteArrow.transform.localPosition;
            voteCountText.transform.eulerAngles = textReference.transform.eulerAngles;

            var text = voteCountText.GetComponent<Il2CppTMPro.TMP_Text>();
            text.SetText("#####");
            text.alignment = Il2CppTMPro.TextAlignmentOptions.Left;

            logger.Debug("Text added");
            return text;
        }
    }
}
