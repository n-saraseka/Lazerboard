window.addEventListener('load', () => {
    const burgerButton = document.querySelector('.burger-icon');
    burgerButton.addEventListener('click', () => {
        if (burgerButton.classList.contains('active')) {
            burgerButton.classList.remove('active');
        }
        else {
            burgerButton.classList.add('active');
        }
    })
})