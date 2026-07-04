// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("click", function (e) {
    var toggle = e.target.closest(".reply-toggle, .edit-toggle");
    if (!toggle) return;
    e.preventDefault();
    var target = document.getElementById(toggle.dataset.target);
    if (target) target.classList.toggle("d-none");
});

function evaluatePasswordStrength(password) {
    var criteria = [
        password.length >= 8,
        /[A-Z]/.test(password),
        /[a-z]/.test(password),
        /[0-9]/.test(password),
        /[\W_]/.test(password)
    ];
    var score = criteria.filter(Boolean).length;

    if (score <= 2) return { level: "Débil", percent: 33, css: "bg-danger" };
    if (score <= 4) return { level: "Media", percent: 66, css: "bg-warning" };
    return { level: "Fuerte", percent: 100, css: "bg-success" };
}

function wirePasswordStrengthMeter(inputId, barId, textId) {
    var input = document.getElementById(inputId);
    var bar = document.getElementById(barId);
    var text = document.getElementById(textId);
    if (!input || !bar || !text) return;

    input.addEventListener("input", function () {
        if (!input.value) {
            bar.style.width = "0%";
            bar.className = "progress-bar bg-danger";
            text.textContent = "Fortaleza: Ninguna";
            return;
        }
        var result = evaluatePasswordStrength(input.value);
        bar.style.width = result.percent + "%";
        bar.className = "progress-bar " + result.css;
        text.textContent = "Fortaleza: " + result.level;
    });
}

function wireTogglePassword(buttonId, inputId) {
    var button = document.getElementById(buttonId);
    var input = document.getElementById(inputId);
    if (!button || !input) return;

    button.addEventListener("click", function () {
        var isHidden = input.type === "password";
        input.type = isHidden ? "text" : "password";
        button.textContent = isHidden ? "Ocultar" : "Mostrar";
    });
}
