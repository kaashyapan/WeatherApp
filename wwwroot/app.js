window.resetForm = (formId) => {
  const form = document.getElementById(formId)
  
  form.reset();
  
  form.querySelectorAll('input').forEach((el) => {
    el.value = ""
  })
  
  form.querySelectorAll('.form-error').forEach((el) => {
    el.remove()
  })
}
