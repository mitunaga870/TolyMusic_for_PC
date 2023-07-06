var list = document.getElementsByName("tp-yt-paper-listbox")[0];
var item = document.createElement("li");
var button = document.createElement("button");
button.innerHTML = "Click Me";
button.onclick = function () {
    alert("Hello World");
};
item.appendChild(button);
list.appendChild(item);