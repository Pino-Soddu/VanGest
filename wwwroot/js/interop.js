function isMobileDevice() {
    return window.innerWidth < 768;
}

function adjustFilterWidth() {
    const filter = document.querySelector('.advanced-filter-compact');
    if (window.innerWidth < 768) {
        filter.style.width = '92%';
    } else {
        filter.style.width = '240px';
    }
}