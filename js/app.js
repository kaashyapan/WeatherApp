const resetForm = (formId) => {
	const form = document.getElementById(formId);
	if (form !== null && form !== undefined) {
		form.reset();
	}
	// form.querySelectorAll("input").forEach((el) => {
	// 	el.value = "";
	// });
	let errors = form.querySelectorAll(".form-error");
	if (errors !== null && form !== undefined) {
		errors.forEach((el) => {
			el.remove();
		});
	}
};

const submitLoginForm = (formId) => {
	const form = document.getElementById(formId);
	if (form !== null && form !== undefined) {
		form.action = "/signin" + window.location.search;
		form.method = "post";
		form.submit();
	}
};

const killMyself = (elId) => {
	const el = document.getElementById(elId);
	if (el !== null && el !== undefined) {
		el.remove();
	}
};

const killMyselfLater = (elId) => setTimeout(() => killMyself(elId), 5000);

export { resetForm, killMyself, killMyselfLater, submitLoginForm };
