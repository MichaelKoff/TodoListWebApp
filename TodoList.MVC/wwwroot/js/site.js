let currentTab = 'not-started-button';

function updateCurrentTab() {
    let tablinks = document.getElementsByClassName("tablinks");
    for (const element of tablinks) {
        if (element.className.includes("active")) {
            currentTab = element.id;
            break;
        }
    }
}