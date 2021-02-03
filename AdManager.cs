using UnityEngine.Events;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using LevelManagement.Data;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;

    bool UserWantRewardButNotLoaded = false;
    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {


        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });

        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
                    Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) //internet is connected
        {
            LoadRewardVideo();
            LoadInterstitialAd();

        }


        StartCoroutine(LoadAdsContinuously());

    }

    IEnumerator LoadAdsContinuously()
    {
        while (true)
        {
            if (!rewardedAd.IsLoaded())
            {
                if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
                    Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
                {
                    LoadRewardVideo();
                }
            }
            if (!interstitialAd.IsLoaded())
            {
                if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||
                    Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
                {
                    LoadInterstitialAd();
                }
            }
            yield return new WaitForSeconds(8f);
        }
    }


    public void LoadInterstitialAd()
    {
        string adUnitId = "ca-app-pub-3940256099942544/1033173712"; // interstitial test id
        this.interstitialAd = new InterstitialAd(adUnitId);

        AdRequest request = new AdRequest.Builder().Build();
        this.interstitialAd.LoadAd(request);

        // Called when an ad request has successfully loaded.
        this.interstitialAd.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstitialAd.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        this.interstitialAd.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        this.interstitialAd.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        this.interstitialAd.OnAdLeavingApplication += HandleOnAdLeavingApplication;
    }

    public void WatchInterstitialAd()
    {

        if (interstitialAd.IsLoaded())
        {
            interstitialAd.Show();

        }
        else
        {
            Debug.Log("Interstitial ad not loaded");
        }
    }
    public void LoadRewardVideo()
    {
        string adUnitId = "ca-app-pub-3940256099942544/5224354917";  //reward test id
        this.rewardedAd = new RewardedAd(adUnitId);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);


        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

    }
    public void UserChoseToWatchAd()
    {
        if (this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
        }
        else
        {
            UserWantRewardButNotLoaded = true;
            Debug.Log("ad not loaded yet");
        }
    }



    #region handle events for rewardedvideo ads
    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdLoaded event received");
        if (UserWantRewardButNotLoaded)
        {
            UserWantRewardButNotLoaded = false;
            UserChoseToWatchAd();
        }
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToLoad event received with message: "
                             + args.Message);
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdOpening vent received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.Message);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdClosed event received");
        //Load next add here
        LoadRewardVideo();

    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        //string type = args.Type;
        //double amount = args.Amount;
        //MonoBehaviour.print(
        //    "HandleRewardedAdRewarded event received for "
        //                + amount.ToString() + " " + type);

        Debug.Log("hurray reward mil gya");


        DataManager.instance.carNumber = PlayerPrefs.GetInt("CarNumber");
        DataManager.instance.Save();
      
    }
    #endregion

    #region handle events for Interstitial ads
    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                            + args.Message);
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdClosed event received");
        interstitialAd.Destroy();
        LoadInterstitialAd();
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLeavingApplication event received");
    }
    #endregion
}

