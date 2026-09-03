import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { presentModel } from '../../models/present';
import { PresentService } from '../../services/presentService/present-service';
import { UserService } from '../../services/userService/user-service';
import { RouterLink } from '@angular/router';

// Gift Management Component - CRUD operations for gifts
@Component({
  selector: 'app-present',
  imports: [CommonModule, RouterLink],
  templateUrl: './present.html',
  styleUrl: './present.scss',
})
export class Present {
  
}