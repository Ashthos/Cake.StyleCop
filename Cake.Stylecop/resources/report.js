function toggle(name, img) {
    var element = document.getElementById(name);

    if (element.style.display === "none")
        element.style.display = "";
    else
        element.style.display = "none";

    var imgElement = document.getElementById(img);

    if (imgElement.src.indexOf("minus.png") > 0)
        imgElement.src = "resources/plus.png";
    else
        imgElement.src = "resources/minus.png";
}
