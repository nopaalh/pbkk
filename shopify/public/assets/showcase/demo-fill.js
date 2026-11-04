document.querySelectorAll('[data-demo-email]').forEach((button) => {
  button.addEventListener('click', () => {
    const form = button.closest('form');
    form.elements.email.value = button.dataset.demoEmail;
    form.elements.password.value = button.dataset.demoPassword;
    form.elements.email.focus();
  });
});
