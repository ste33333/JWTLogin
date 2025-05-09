import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  credentials = { username: '', email: '', password: '', confirmPassword: '' };
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;

  constructor(private authService: AuthService, private router: Router) { }

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.isLoading = true;

    if (this.credentials.password !== this.credentials.confirmPassword) {
      this.errorMessage = 'Le password non coincidono.';
      this.isLoading = false;
      return;
    }

    const registrationData = {
      username: this.credentials.username,
      email: this.credentials.email,
      password: this.credentials.password
    };

    this.authService.register(registrationData).subscribe({
      next: (response: any) => { 
        this.isLoading = false;
        this.successMessage = response?.message || 'Registrazione avvenuta con successo! Ora puoi effettuare il login.';
        setTimeout(() => { this.router.navigate(['/login']); }, 3000);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.message || 'Errore durante la registrazione. Riprova.';
        console.error('Errore registrazione:', err);
      }
    });
  }
}