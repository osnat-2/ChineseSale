import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { presentModel } from '../../models/present';
import { cardModel } from '../../models/card';
import { PresentService } from '../../services/presentService/present-service';
import { UserService } from '../../services/userService/user-service';
import { CardService } from '../../services/cardService/card-service';
import { Navbar } from '../navbar/navbar';
import { PersonalArea } from '../personal-area/personal-area';

@Component({
  selector: 'app-home',
  imports: [CommonModule, RouterLink, Navbar, PersonalArea],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  
}