﻿using System;
using System.Collections.Generic;
using SOArchitecture.Channels;
using UnityEngine;

namespace ViewManagement
{
    public class ViewManager : MonoBehaviour
    {
        [SerializeField] private VoidChannelSO backEventChannel;
        [SerializeField] private View startView;
        [SerializeField] private View[] views;

        public readonly Stack<View> activeViewsStack = new Stack<View>();
        private readonly Dictionary<View, Action> actionByView = new Dictionary<View, Action>();

        private void Awake()
        {
            InitializeViews();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBack();
            }
        }

        private void OnEnable()
        {
            backEventChannel.Register(OnBack);
        }

        private void OnDisable()
        {
            backEventChannel.Unregister(OnBack);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < views.Length; i++)
            {
                if (actionByView.ContainsKey(views[i]))
                {
                    views[i].ShowEvent.Unregister((actionByView[views[i]]));
                }
            }
        }
        
        private void InitializeViews()
        {
            foreach (View view in views)
            {
                view.Initialize();
                view.Hide(true);

                if (actionByView.ContainsKey(view))
                {
                    Debug.LogError($"View [{view.name}] is initialized for the secondTime!");
                    return;
                }

                actionByView.Add(view, () => OnShow(view));
                view.ShowEvent.Register(actionByView[view]);
            }

            if (startView != null)
            {
                OnShow(startView);
            }
        }

        private void OnShow(View view)
        {
            while (activeViewsStack.Count > 0)
            {
                View lastView = activeViewsStack.Peek();

                if (lastView == view)
                {
                    ShowView(view);
                    return;
                }

                if (lastView.Depth < view.Depth)
                {
                    if (view.Mode == ViewMode.Swap)
                    {
                        HideView(lastView);
                    }
                    else
                    {
                        ShowView(lastView);
                    }

                    break;
                }

                HideView(activeViewsStack.Pop());
            }

            activeViewsStack.Push(view);
            ShowView(view);
        }

        private void OnBack()
        {
            if (activeViewsStack.Count > 1)
            {
                if (activeViewsStack.Peek().IsLocked)
                    return;

                View lastView = activeViewsStack.Pop();

                if (activeViewsStack.Peek().Depth < lastView.Depth)
                {
                    HideView(lastView);
                    ShowView(activeViewsStack.Peek(), 2);
                    return;
                }

                activeViewsStack.Push(lastView);
            }

            activeViewsStack.Peek().Exit();
        }

        private void ShowView(View view, int siblingOffset = 1)
        {
            if (!view.IsShown)
            {
                view.transform.SetSiblingIndex(view.transform.parent.childCount - siblingOffset);
            }

            view.Show();
        }

        private void HideView(View view)
        {
            view.Hide();
        }

        public void LoadViews()
        {
            views = FindObjectsOfType<View>(true);
        }

        public void ClearViews()
        {
            views = new View[0];
        }
    }
}