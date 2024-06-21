


const popup = document.getElementById("popup");
const popupButton = document.getElementById("popupButton");
const starElements = document.querySelectorAll("#star span");
let selectedRating = 0;

popupButton.addEventListener("click", () => {
    popup.style.display = "block";
    setTimeout(() => {
        document.querySelector('.popup-content').classList.add('show');
    }, 10);
});

function closePopup() {
    document.querySelector('.popup-content').classList.remove('show');
    setTimeout(() => {
        popup.style.display = "none";
    }, 300);
}

starElements.forEach(star => {
    star.addEventListener("click", () => {
        selectedRating = star.getAttribute("data-rating");
        updateStarRating(selectedRating);
    });
});

function updateStarRating(rating) {
    starElements.forEach(star => {
        if (star.getAttribute("data-rating") <= rating) {
            star.classList.add("rank-rated");
        } else {
            star.classList.remove("rank-rated");
        }
    });
    document.getElementById("score").textContent = `您給了 ${rating} 星`;
}

function submitData() {
    const reviewText = document.getElementById("write").value;
    if (selectedRating > 0 && reviewText.trim() !== "") {
        console.log(`評價：${selectedRating} 星`);
        console.log(`評價內容：${reviewText}`);
        alert("謝謝您的評價！");
        closePopup();
    } else {
        alert("請給予星級評價並填寫評價內容！");
    }
}
