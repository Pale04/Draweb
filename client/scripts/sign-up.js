const usernameInput = document.getElementById('username')
const emailInput = document.getElementById('email')
const passwordInput = document.getElementById('password')
const form = document.querySelector('form')
const errors = document.getElementsByClassName('error')

usernameInput.addEventListener('input', () => checkUsernameValidity())
emailInput.addEventListener('input', () => checkEmailValidity())
passwordInput.addEventListener('input', () => checkPasswordValidity())

function checkUsernameValidity() {
    if (usernameInput.validity.valid) {
        errors[0].innerHTML = ''
        return true
    } else {
        errors[0].textContent = 'Username required'
        return false
    }
}

function checkEmailValidity() {
    if (emailInput.validity.valid) {
        errors[1].innerHTML = ''
        return true
    } else {
        errors[1].textContent = 'Enter a valid email'
        return false
    }
}

function checkPasswordValidity() {
    if (passwordInput.validity.valid) {
        errors[2].innerHTML = ''
        return true
    } else {
        errors[2].textContent = 'Password required'
        return false
    }
}

form.addEventListener('submit', (event) => {
    const usernameIsValid = checkUsernameValidity()
    const emailIsValid = checkEmailValidity()
    const passwordIsValid = checkPasswordValidity()
    event.preventDefault()
    if(usernameIsValid && emailIsValid && passwordIsValid) {
        sendLoginForm()
    }
})

async function sendLoginForm() {
    try {
        const response = await fetch('http://localhost:5043/draweb-api/users', {
            method: 'POST',
            body: JSON.stringify({
                username: usernameInput.value,
                password: passwordInput.value,
                email: emailInput.value
            }),
            headers: {
                'Content-Type': 'application/json'
            }
        })
        const responseCode = response.status
        
        if (responseCode == 201) {
            errors[3].innerHTML = ''
            alert('The account was created successfuly')
        } else if (responseCode == 409) {
            errors[3].innerHTML = 'The username or email belongs to an existent account'
        } else {
            errors[3].innerHTML = 'Somenthin went wrong'
        }
    } catch (e) {
        errors[3].textContent = 'Something went wrong'
    }
}
