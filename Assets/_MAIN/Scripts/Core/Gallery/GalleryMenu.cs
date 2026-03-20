using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GalleryMenu : MonoBehaviour
{
    private const int PAGE_BUTTON_LIMIT = 2;
    private int maxPages = 0;
    private int selectedPage = 0;

    [SerializeField] private CanvasGroup root;
    private CanvasGroupController rootCG;

    [SerializeField] private Texture[] galleryImages;

    [SerializeField] private Button[] galleryPreviewButtons;
    [SerializeField] private Button panelSelectionButtonPrefab;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [SerializeField] private CanvasGroup previewPanel;
    private CanvasGroupController previewPanelCG;

    [SerializeField] Button previewButton;

    private bool initialized = false;
    private int previewsPerPage => galleryPreviewButtons.Length;

    // Start is called before the first frame update
    void Start()
    {
        rootCG = new CanvasGroupController(this, root);
        previewPanelCG = new CanvasGroupController(this, previewPanel);

        GalleryConfig.Load();

        GetAllGalleryImages();
    }

    public void Open()
    {
        if (!initialized)
            Initialize();

        rootCG.Show();
        rootCG.SetInteractableState(true);
    }

    public void Close()
    {
        rootCG?.Hide();
        rootCG.SetInteractableState(false);
    }

    private void GetAllGalleryImages()
    {
        galleryImages = Resources.LoadAll<Texture>(FilePaths.resources_gallery);
    }

    private void Initialize()
    {
        initialized = true;

        ConstructNavBar();

        LoadPage(1);
    }

    private void ConstructNavBar()
    {
        int totalImages = galleryImages.Length;

        maxPages = ((int)Mathf.Ceil((float)totalImages / previewsPerPage));
        int pagelimit = PAGE_BUTTON_LIMIT < maxPages ? PAGE_BUTTON_LIMIT : maxPages;

        for (int i = 1; i <= pagelimit; i++)
        {
            GameObject buttonOB = Instantiate(panelSelectionButtonPrefab.gameObject, panelSelectionButtonPrefab.transform.parent);
            buttonOB.SetActive(true);

            Button button = buttonOB.GetComponent<Button>();
            TextMeshProUGUI txt = button.GetComponentInChildren<TextMeshProUGUI>();

            buttonOB.name = i.ToString();
            int page = i;
            button.onClick.AddListener(() => LoadPage(page));
            txt.text = i.ToString();
        }

        //prevButton.gameObject.SetActive(pagelimit < maxPages);
        //nextButton.gameObject.SetActive(pagelimit < maxPages);

        prevButton.gameObject.SetActive(maxPages > 1);
        nextButton.gameObject.SetActive(maxPages > 1);

        nextButton.transform.SetAsLastSibling();
    }

    private void LoadPage(int pageNumber)
    {
        int startingIndex = (pageNumber - 1) * previewsPerPage;

        for (int i = 0; i < previewsPerPage; i++)
        {
            int index = i + startingIndex;
            Button button = galleryPreviewButtons[i];

            if (button == null)
                continue; // Skip if button is not assigned (if only have 1 gallery page)

            button.onClick.RemoveAllListeners();

            if (index >= galleryImages.Length)
            {
                button.transform.parent.gameObject.SetActive(false);
                continue;
            }
            else
            {
                button.transform.parent.gameObject.SetActive(true);
                RawImage renderer = button.targetGraphic as RawImage;
                Texture previewImage = galleryImages[index];

                if (renderer == null)
                    continue; // Skip if renderer is not assigned

                if (GalleryConfig.ImageIsUnlocked(previewImage.name))
                {
                    renderer.color = Color.white;
                    renderer.texture = previewImage;
                    button.onClick.AddListener(() => ShowPreviewImage(previewImage));
                }
                else
                {
                    renderer.color = Color.black;
                    renderer.texture = null;
                }
            }
        }

        selectedPage = pageNumber;
    }

    private void ShowPreviewImage(Texture image)
    {
        RawImage renderer = previewButton.targetGraphic as RawImage;
        renderer.texture = image;
        previewPanelCG.Show();
        previewPanelCG.SetInteractableState(true);
    }

    public void HidePreviewImage()
    {
        previewPanelCG.Hide();
        previewPanelCG.SetInteractableState(false);
    }

    public void ToNextPage()
    {
        if (selectedPage < maxPages)
            LoadPage(selectedPage + 1);
    }

    public void ToPreviousPage()
    {
        if (selectedPage > 1)
            LoadPage(selectedPage - 1);
    }
}
