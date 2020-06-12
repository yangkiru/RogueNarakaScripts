using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

// Placing the Purchaser class in the CompleteProject namespace allows it to interact with ScoreManager, 
// one of the existing Survival Shooter scripts.
// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager instance { get; set; }

    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

    public const string PRODUCT_1000_SOUL = "soul.1000";
    public const string PRODUCT_5500_SOUL = "soul.5500";
    public const string PRODUCT_12000_SOUL = "soul.12000";
    public const string PRODUCT_REMOVE_AD = "ad.remove";

    public const string PRODUCT_10_DIAMOND = "diamond.10";
    public const string PRODUCT_23_DIAMOND = "diamond.23";
    public const string PRODUCT_36_DIAMOND = "diamond.36";
    public const string PRODUCT_62_DIAMOND = "diamond.62";
    public const string PRODUCT_124_DIAMOND = "diamond.124";
    public const string PRODUCT_250_DIAMOND = "diamond.250";
    public const string PRODUCT_380_DIAMOND = "diamond.380";
    public const string PRODUCT_650_DIAMOND = "diamond.650";
    public const string PRODUCT_1500_DIAMOND = "diamond.1500";

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }

        //CheckProducts();
    }

    public void InitializePurchasing()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }

        // Create a builder, first passing in a suite of Unity provided stores.
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct("soul.1000", ProductType.Consumable);
        builder.AddProduct("soul.5500", ProductType.Consumable);
        builder.AddProduct("soul.12000", ProductType.Consumable);
        builder.AddProduct("ad.remove", ProductType.NonConsumable);
        builder.AddProduct(PRODUCT_10_DIAMOND, ProductType.Consumable);
        builder.AddProduct(PRODUCT_23_DIAMOND, ProductType.Consumable);
        builder.AddProduct(PRODUCT_36_DIAMOND, ProductType.Consumable);
        builder.AddProduct(PRODUCT_62_DIAMOND, ProductType.Consumable);
        builder.AddProduct(PRODUCT_124_DIAMOND, ProductType.Consumable);
        builder.AddProduct(PRODUCT_250_DIAMOND, ProductType.Consumable);
        builder.AddProduct(PRODUCT_380_DIAMOND, ProductType.Consumable);
        builder.AddProduct(PRODUCT_650_DIAMOND, ProductType.Consumable);
        builder.AddProduct(PRODUCT_1500_DIAMOND, ProductType.Consumable);
        //builder.AddProduct(kProductIDSubscription, ProductType.Subscription, new IDs(){
        //        { kProductNameAppleSubscription, AppleAppStore.Name },
        //        { kProductNameGooglePlaySubscription, GooglePlay.Name },
        //    });

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void BuySoul(int amount)
    {
        switch (amount)
        {
            case 1000:
                BuyProductID(PRODUCT_1000_SOUL);
                break;
            case 5500:
                BuyProductID(PRODUCT_5500_SOUL);
                break;
            case 12000:
                BuyProductID(PRODUCT_12000_SOUL);
                break;
            default:
                Debug.Log("Tried to buy the wrong product.");
                break;
        }
    }

    public void BuyDiamond(int _amount) {
        switch(_amount) {
            case 10:
                BuyProductID(PRODUCT_10_DIAMOND);
                break;
            case 23:
                BuyProductID(PRODUCT_23_DIAMOND);
                break;
            case 36:
                BuyProductID(PRODUCT_36_DIAMOND);
                break;
            case 62:
                BuyProductID(PRODUCT_62_DIAMOND);
                break;
            case 124:
                BuyProductID(PRODUCT_124_DIAMOND);
                break;
            case 250:
                BuyProductID(PRODUCT_250_DIAMOND);
                break;
            case 380:
                BuyProductID(PRODUCT_380_DIAMOND);
                break;
            case 650:
                BuyProductID(PRODUCT_650_DIAMOND);
                break;
            case 1500:
                BuyProductID(PRODUCT_1500_DIAMOND);
                break;
            default:
                Debug.Log("Tried to buy the wrong product.");
                break;
        }
    }

    public void BuyAdRemove()
    {
        BuyProductID(PRODUCT_REMOVE_AD);
    }

    private void BuyProductID(string productId)
    {
        // If Purchasing has been initialized ...
        if (IsInitialized())
        {
            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = m_StoreController.products.WithID(productId);

            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.
                m_StoreController.InitiatePurchase(product);
            }
            // Otherwise ...
            else
            {
                // ... report the product look-up failure situation  
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        // Otherwise ...
        else
        {
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
    // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
    //public void RestorePurchases()
    //{
    //    // If Purchasing has not yet been set up ...
    //    if (!IsInitialized())
    //    {
    //        // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
    //        Debug.Log("RestorePurchases FAIL. Not initialized.");
    //        return;
    //    }

    //    // If we are running on an Apple device ... 
    //    if (Application.platform == RuntimePlatform.IPhonePlayer ||
    //        Application.platform == RuntimePlatform.OSXPlayer)
    //    {
    //        // ... begin restoring purchases
    //        Debug.Log("RestorePurchases started ...");

    //        // Fetch the Apple store-specific subsystem.
    //        var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
    //        // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
    //        // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
    //        apple.RestoreTransactions((result) =>
    //        {
    //            // The first phase of restoration. If no more responses are received on ProcessPurchase then 
    //            // no purchases are available to be restored.
    //            Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
    //        });
    //    }
    //    // Otherwise ...
    //    else
    //    {
    //        // We are not running on an Apple device. No work is necessary to restore purchases.
    //        Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
    //    }
    //}


    //  
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;
        CheckADRemove();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }


    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        // A consumable product has been purchased by this user.

        switch(args.purchasedProduct.definition.id)
        {
            case PRODUCT_1000_SOUL:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddSoul(1000);
                else
                    PlayerPrefs.SetInt("soul", PlayerPrefs.GetInt("soul") + 1000);
                break;
            case PRODUCT_5500_SOUL:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddSoul(5500);
                else
                    PlayerPrefs.SetInt("soul", PlayerPrefs.GetInt("soul") + 5500);
                break;
            case PRODUCT_12000_SOUL:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddSoul(12000);
                else
                    PlayerPrefs.SetInt("soul", PlayerPrefs.GetInt("soul") + 12000);
                break;
            case PRODUCT_REMOVE_AD:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                AdMobManager.instance.RemoveAds();
                break;
            //Purcahse Diamond
            case PRODUCT_10_DIAMOND:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddJewel(10);
                else
                    PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 10);
                break;
            case PRODUCT_23_DIAMOND:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddJewel(23);
                else
                    PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 23);
                break;
            case PRODUCT_36_DIAMOND:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddJewel(36);
                else
                    PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 36);
                break;
            case PRODUCT_62_DIAMOND:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddJewel(62);
                else
                    PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 62);
                break;
            case PRODUCT_124_DIAMOND:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddJewel(124);
                else
                    PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 124);
                break;
            case PRODUCT_250_DIAMOND:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddJewel(250);
                else
                    PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 250);
                break;
            case PRODUCT_380_DIAMOND:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddJewel(380);
                else
                    PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 380);
                break;
            case PRODUCT_650_DIAMOND:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddJewel(650);
                else
                    PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 650);
                break;
            case PRODUCT_1500_DIAMOND:
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                if (MoneyManager.instance) MoneyManager.instance.AddJewel(1500);
                else
                    PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 1500);
                break;
            //
            default:
                Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
                break;
        }

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        return PurchaseProcessingResult.Complete;
    }

    void CheckProducts()
    {
        CheckProduct(PRODUCT_1000_SOUL);
        CheckProduct(PRODUCT_5500_SOUL);
        CheckProduct(PRODUCT_12000_SOUL);
        //Diamond
        CheckProduct(PRODUCT_10_DIAMOND);
        CheckProduct(PRODUCT_23_DIAMOND);
        CheckProduct(PRODUCT_36_DIAMOND);
        CheckProduct(PRODUCT_62_DIAMOND);
        CheckProduct(PRODUCT_124_DIAMOND);
        CheckProduct(PRODUCT_250_DIAMOND);
        CheckProduct(PRODUCT_380_DIAMOND);
        CheckProduct(PRODUCT_650_DIAMOND);
        CheckProduct(PRODUCT_1500_DIAMOND);
        //
        CheckProduct(PRODUCT_REMOVE_AD);
    }

    void CheckADRemove()
    {
        if (PlayerPrefs.GetInt("isRemoveAds") == 1)
        {
            Product product = m_StoreController.products.WithID(PRODUCT_REMOVE_AD);
            if (product != null && !product.hasReceipt)
            {
                PlayerPrefs.SetInt("isRemoveAds", 0);
            }
        }
        else
        {
            Product product = m_StoreController.products.WithID(PRODUCT_REMOVE_AD);
            if (product != null && product.hasReceipt)
            {
                PlayerPrefs.SetInt("isRemoveAds", 1);
            }
        }
    }

    void CheckProduct(string productID)
    {
        //#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log(string.Format("CheckProduct:{0}", productID));
        StartCoroutine(CheckProductCorou(productID));
//#endif
    }

    IEnumerator CheckProductCorou(string productID)
    {
        float t = 5;
        do
        {
            yield return null;
            t -= Time.unscaledDeltaTime;
        } while (t > 0);
        
        
        Debug.Log(string.Format("CheckingProduct:{0}", productID));

        Product cproduct = m_StoreController.products.WithID(productID);
        if(cproduct != null && cproduct.hasReceipt)
        {
            switch (productID)
            {
                case PRODUCT_1000_SOUL:
                    Debug.Log(string.Format("RestorePurchase: PASS. Product: '{0}'", productID));
                    if(MoneyManager.instance) MoneyManager.instance.AddSoul(1000);
                    else
                        PlayerPrefs.SetInt("soul", PlayerPrefs.GetInt("soul") + 1000);
                    break;
                case PRODUCT_5500_SOUL:
                    Debug.Log(string.Format("RestorePurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddSoul(5500);
                    else
                        PlayerPrefs.SetInt("soul", PlayerPrefs.GetInt("soul") + 5500);
                    break;
                case PRODUCT_12000_SOUL:
                    Debug.Log(string.Format("RestorePurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddSoul(12000); 
                    else
                        PlayerPrefs.SetInt("soul", PlayerPrefs.GetInt("soul") + 12000);
                    break;
                //Purcahse Diamond
                case PRODUCT_10_DIAMOND:
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddJewel(10);
                    else
                        PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 10);
                    break;
                case PRODUCT_23_DIAMOND:
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddJewel(23);
                    else
                        PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 23);
                    break;
                case PRODUCT_36_DIAMOND:
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddJewel(36);
                    else
                        PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 36);
                    break;
                case PRODUCT_62_DIAMOND:
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddJewel(62);
                    else
                        PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 62);
                    break;
                case PRODUCT_124_DIAMOND:
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddJewel(124);
                    else
                        PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 124);
                    break;
                case PRODUCT_250_DIAMOND:
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddJewel(250);
                    else
                        PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 250);
                    break;
                case PRODUCT_380_DIAMOND:
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddJewel(380);
                    else
                        PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 380);
                    break;
                case PRODUCT_650_DIAMOND:
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddJewel(650);
                    else
                        PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 650);
                    break;
                case PRODUCT_1500_DIAMOND:
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productID));
                    if (MoneyManager.instance) MoneyManager.instance.AddJewel(1500);
                    else
                        PlayerPrefs.SetInt("jewel", PlayerPrefs.GetInt("jewel") + 1500);
                    break;
                case PRODUCT_REMOVE_AD:
                    Debug.Log(string.Format("RestorePurchase: PASS. Product: '{0}'", productID));
                    AdMobManager.instance.RemoveAds();
                    break;
                default:
                    Debug.Log(string.Format("RestorePurchase: FAIL. Unrecognized id: '{0}'", productID));
                    break;
            }
        }
        else
            Debug.Log(string.Format("RestorePurchase: FAIL. Unrecognized product: '{0}'", productID));
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }
}