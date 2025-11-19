const usernameInput = document.getElementById('username')
const passwordInput = document.getElementById('password')
const form = document.querySelector('form')
const errors = document.getElementsByClassName('error')

usernameInput.addEventListener('input', () => checkUsernameValidity())
passwordInput.addEventListener('input', () => checkPasswordValidity())

form.addEventListener('submit', (event) => {
    const usernameIsValid = checkUsernameValidity()
    const passwordIsValid = checkPasswordValidity()
    event.preventDefault()
    if(usernameIsValid && passwordIsValid) {
        sendLoginForm()
    }
})

function checkUsernameValidity() {
    if (usernameInput.validity.valid) {
        errors[0].innerHTML = ''
        return true
    } else {
        errors[0].textContent = 'Username required'
        return false
    }
}

function checkPasswordValidity() {
    if (passwordInput.validity.valid) {
        errors[1].innerHTML = ''
        return true
    } else {
        errors[1].textContent = 'Password required'
        return false
    }
}

async function sendLoginForm() {
    try {
        const response = await fetch("http://localhost:5043/draweb-api/authenticate", {
            method: 'POST',
            body: JSON.stringify({
                username: usernameInput.value,
                password: passwordInput.value
            }),
            headers: {
                'Content-Type': 'application/json'
            }
        });
        const responseCode = response.status
        if (responseCode == 200) {
            errors[2].innerHTML = ''
            const body = await response.json()
            sessionStorage.setItem('jwt_token', body.token);
            window.location.replace('home.html');
        } else if (responseCode == 401) {
            errors[2].textContent = 'Incorrect credentials'
        } else {
            errors[2].textContent = 'Something went wrong'
        }
    } catch (e) {
        console.error(e);
        errors[2].textContent = 'Something went wrong'
    }
}
