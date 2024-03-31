document.getElementById("form").addEventListener('submit', (e) => {
    e.preventDefault();

    const titleInput = document.getElementById("titleInput")
    const fileInput = document.getElementById("fileInput")

    const titleValidation = document.getElementById("title-validation")
    const fileValidation = document.getElementById("file-validation")

    titleValidation.innerText = ""
    fileValidation.innerText = ""

    var isValid = true

    if (titleInput.value === "") {
        titleValidation.innerText = "Title is required"
        isValid = false
    }
    if (fileInput.value === "" || !/\.(jpg|jpeg|png|gif)$/.test(fileInput.value)) {
        fileValidation.innerText = "Please select a file"
        isValid = false
    }
    if (!isValid) { return}

    var formData = new FormData();
    formData.append("title", titleInput.value)
    formData.append("file", fileInput.files[0], fileInput.files[0].name)


    fetch("/upload", {
        method: "POST",
        body: formData
    })
        .then(response => response.json())
        .then(data => { window.location.href = "/picture/" + data.id })
        .catch(error => console.error("Error:", error))
})